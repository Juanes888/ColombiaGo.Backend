-- ColombiaGO — schema (run once in the Supabase SQL Editor, or via `psql`)
--
-- Naming mirrors what EF Core produces automatically via EFCore.NamingConventions
-- (snake_case tables/columns) and Npgsql's HasPostgresEnum<TEnum>() (snake_case enum type
-- names AND member labels — e.g. C# `EstadoEvento.EnCurso` -> Postgres label 'en_curso').
-- If you rename a C# entity, property, or enum member, this file — not an EF migration —
-- is the source of truth to update; the app never runs `Database.Migrate()`.
--
-- Idempotent: safe to re-run. Uses IF NOT EXISTS / DROP ... IF EXISTS guards throughout.

-- ============================================================================
-- Extensions
-- ============================================================================

-- Accelerates the ILIKE '%term%' search LugarRepository.SearchAsync runs (a plain B-tree/GIN
-- can't help with a leading wildcard; trigram indexes can).
create extension if not exists pg_trgm;

-- ============================================================================
-- Enum types
-- ============================================================================

do $$ begin
    create type rango_precio as enum ('gratis', 'economico', 'moderado', 'costoso', 'lujo');
exception when duplicate_object then null; end $$;

do $$ begin
    create type estado_publicacion as enum ('pendiente', 'aprobado', 'rechazado', 'archivado');
exception when duplicate_object then null; end $$;

do $$ begin
    create type estado_moderacion as enum ('pendiente', 'aprobada', 'rechazada');
exception when duplicate_object then null; end $$;

do $$ begin
    create type estado_evento as enum ('programado', 'en_curso', 'finalizado', 'cancelado');
exception when duplicate_object then null; end $$;

do $$ begin
    create type tipo_interaccion as enum ('vista', 'busqueda', 'click', 'compartido', 'favorito');
exception when duplicate_object then null; end $$;

do $$ begin
    create type tipo_dato as enum ('texto', 'numero', 'booleano', 'fecha', 'lista');
exception when duplicate_object then null; end $$;

do $$ begin
    create type tipo_reporte as enum ('usuarios', 'lugares', 'calificaciones', 'eventos', 'general');
exception when duplicate_object then null; end $$;

do $$ begin
    create type formato_salida as enum ('pdf', 'excel', 'csv');
exception when duplicate_object then null; end $$;

-- ============================================================================
-- Reference tables
-- ============================================================================

create table if not exists roles (
    id          serial primary key,
    nombre      varchar(50) not null unique,
    descripcion varchar(200)
);

create table if not exists categorias (
    id     serial primary key,
    nombre varchar(80) not null unique,
    slug   varchar(80) not null unique,
    icono  varchar(100),
    tono   varchar(30),
    activa boolean not null default true
);

create table if not exists logros (
    id          serial primary key,
    titulo      varchar(150) not null,
    descripcion varchar(500) not null,
    icono       varchar(50) not null,
    tono        varchar(30) not null,
    criterio    varchar(300)
);

-- ============================================================================
-- Usuarios — extends auth.users (Supabase-managed). Id is never generated here;
-- it always mirrors an existing auth.users.id (see UsuarioProvisioner / handle_new_user below).
-- ============================================================================

create table if not exists usuarios (
    id                        uuid primary key references auth.users (id) on delete cascade,
    rol_id                    integer not null references roles (id) on delete restrict,
    nombre_completo           varchar(200) not null,
    correo_electronico        varchar(320) not null unique,
    foto_perfil_url           varchar(500),
    acepto_terminos           boolean not null default false,
    fecha_aceptacion_terminos timestamptz,
    activo                    boolean not null default true,
    nivel                     varchar(50) not null default 'Viajero Nivel 1',
    puntos                    integer not null default 0,
    fecha_registro            timestamptz not null default now(),
    fecha_actualizacion       timestamptz not null default now()
);

create index if not exists ix_usuarios_rol_id on usuarios (rol_id);

create table if not exists usuario_preferencias (
    id           serial primary key,
    usuario_id   uuid not null references usuarios (id) on delete cascade,
    categoria_id integer not null references categorias (id) on delete cascade,
    unique (usuario_id, categoria_id)
);

create table if not exists usuario_configuracion (
    id                  uuid primary key references usuarios (id) on delete cascade,
    notificaciones      boolean not null default true,
    ubicacion           boolean not null default true,
    reduce_motion       boolean not null default false,
    fecha_actualizacion timestamptz not null default now()
);

create table if not exists guias (
    id                     serial primary key,
    -- Null until this directory listing is claimed/linked by a real Supabase account (see
    -- Guia.cs) — lets the demo guides (Mateo, Sofía, Andrés, Lucía) be seeded without one.
    usuario_id             uuid references usuarios (id) on delete set null,
    nombre                 varchar(200) not null,
    foto_url               varchar(500),
    especialidad           varchar(150) not null,
    etiqueta               varchar(60) not null,
    tono_etiqueta          varchar(30) not null,
    insignia               varchar(150),
    lugar                  varchar(150) not null,
    tarifa_hora            numeric(12, 2) not null,
    calificacion_promedio  numeric(3, 2) not null default 0,
    total_calificaciones   integer not null default 0,
    idiomas                text[] not null default '{}',
    destacado              boolean not null default false,
    verificado             boolean not null default false,
    fecha_registro         timestamptz not null default now()
);

-- A real account can only back one guide profile; many unlinked directory entries are fine.
create unique index if not exists ux_guias_usuario_id on guias (usuario_id) where usuario_id is not null;

create table if not exists usuario_logros (
    id             serial primary key,
    usuario_id     uuid not null references usuarios (id) on delete cascade,
    logro_id       integer not null references logros (id) on delete cascade,
    fecha_obtenido timestamptz not null default now(),
    unique (usuario_id, logro_id)
);

-- ============================================================================
-- Category-scoped dynamic attributes (EAV) and features
-- ============================================================================

create table if not exists tipos_atributo (
    id              serial primary key,
    categoria_id    integer not null references categorias (id) on delete cascade,
    nombre          varchar(100) not null,
    etiqueta        varchar(150) not null,
    tipo_dato       tipo_dato not null,
    es_filtrable    boolean not null default false,
    es_obligatorio  boolean not null default false
);

create index if not exists ix_tipos_atributo_categoria_id on tipos_atributo (categoria_id);

create table if not exists caracteristicas (
    id           serial primary key,
    categoria_id integer not null references categorias (id) on delete cascade,
    nombre       varchar(100) not null
);

create index if not exists ix_caracteristicas_categoria_id on caracteristicas (categoria_id);

-- ============================================================================
-- Lugares
-- ============================================================================

create table if not exists lugares (
    id                     serial primary key,
    categoria_id           integer not null references categorias (id) on delete restrict,
    -- Null for system/seed-provided places (see 002_seed.sql) — there is no human registrant
    -- to point to until real community submissions exist.
    registrado_por         uuid references usuarios (id) on delete set null,
    nombre                 varchar(200) not null,
    slug                   varchar(220) not null unique,
    ciudad                 varchar(120) not null,
    region                 varchar(120) not null,
    descripcion            varchar(4000) not null,
    eslogan                varchar(200),
    direccion              varchar(300) not null,
    latitud                numeric(9, 6) not null,
    longitud               numeric(9, 6) not null,
    horario_atencion       varchar(150),
    como_llegar            varchar(150),
    imagen_portada_url     varchar(500),
    calificacion_promedio  numeric(3, 2) not null default 0,
    total_calificaciones   integer not null default 0,
    rango_precio           rango_precio not null default 'gratis',
    costo_entrada          numeric(12, 2),
    destacado              boolean not null default false,
    estado_publicacion     estado_publicacion not null default 'pendiente',
    motivo_rechazo         varchar(500),
    fecha_registro         timestamptz not null default now(),
    fecha_actualizacion    timestamptz not null default now()
);

create index if not exists ix_lugares_categoria_estado on lugares (categoria_id, estado_publicacion);
create index if not exists ix_lugares_ciudad on lugares (ciudad);
create index if not exists ix_lugares_destacado on lugares (destacado);
create index if not exists ix_lugares_nombre_trgm on lugares using gin (nombre gin_trgm_ops);
create index if not exists ix_lugares_descripcion_trgm on lugares using gin (descripcion gin_trgm_ops);

create table if not exists lugar_atributos (
    id               serial primary key,
    lugar_id         integer not null references lugares (id) on delete cascade,
    tipo_atributo_id integer not null references tipos_atributo (id) on delete cascade,
    valor            varchar(500) not null,
    unique (lugar_id, tipo_atributo_id)
);

create table if not exists lugar_caracteristicas (
    id                serial primary key,
    lugar_id          integer not null references lugares (id) on delete cascade,
    caracteristica_id integer not null references caracteristicas (id) on delete cascade,
    unique (lugar_id, caracteristica_id)
);

create table if not exists galeria_fotos (
    id          serial primary key,
    lugar_id    integer not null references lugares (id) on delete cascade,
    url_imagen  varchar(500) not null,
    orden       integer not null default 0
);

create index if not exists ix_galeria_fotos_lugar_id on galeria_fotos (lugar_id);

-- ============================================================================
-- Eventos
-- ============================================================================

create table if not exists eventos (
    id               serial primary key,
    admin_id         uuid not null references usuarios (id) on delete restrict,
    lugar_id         integer references lugares (id) on delete set null,
    nombre_evento    varchar(200) not null,
    fecha_evento     date not null,
    hora_evento      time not null,
    direccion_evento varchar(300) not null,
    costo            numeric(12, 2),
    descripcion      varchar(2000),
    estado           estado_evento not null default 'programado',
    fecha_creacion   timestamptz not null default now()
);

create index if not exists ix_eventos_fecha_evento on eventos (fecha_evento);
create index if not exists ix_eventos_lugar_id on eventos (lugar_id);

-- ============================================================================
-- Calificaciones, favoritos, historial, recomendaciones
-- ============================================================================

create table if not exists calificaciones (
    id                serial primary key,
    usuario_id        uuid not null references usuarios (id) on delete cascade,
    lugar_id          integer not null references lugares (id) on delete cascade,
    puntuacion        integer not null check (puntuacion between 1 and 5),
    comentario        varchar(2000),
    estado_moderacion estado_moderacion not null default 'pendiente',
    fecha             timestamptz not null default now(),
    unique (usuario_id, lugar_id)
);

create index if not exists ix_calificaciones_lugar_estado on calificaciones (lugar_id, estado_moderacion);

create table if not exists favoritos_lugares (
    id              serial primary key,
    usuario_id      uuid not null references usuarios (id) on delete cascade,
    lugar_id        integer not null references lugares (id) on delete cascade,
    fecha_agregado  timestamptz not null default now(),
    unique (usuario_id, lugar_id)
);

create table if not exists favoritos_eventos (
    id              serial primary key,
    usuario_id      uuid not null references usuarios (id) on delete cascade,
    evento_id       integer not null references eventos (id) on delete cascade,
    fecha_agregado  timestamptz not null default now(),
    unique (usuario_id, evento_id)
);

create table if not exists historial_interacciones (
    id                serial primary key,
    usuario_id        uuid not null references usuarios (id) on delete cascade,
    lugar_id          integer not null references lugares (id) on delete cascade,
    tipo_interaccion  tipo_interaccion not null,
    fecha             timestamptz not null default now()
);

create index if not exists ix_historial_usuario_fecha on historial_interacciones (usuario_id, fecha);

create table if not exists recomendaciones (
    id              serial primary key,
    usuario_id      uuid not null references usuarios (id) on delete cascade,
    lugar_id        integer not null references lugares (id) on delete cascade,
    score           numeric(5, 2) not null,
    motivo          varchar(300),
    fecha_generada  timestamptz not null default now()
);

create index if not exists ix_recomendaciones_usuario_fecha on recomendaciones (usuario_id, fecha_generada);

-- ============================================================================
-- Rutas — fecha_generacion is null while the route is still an open draft
-- (see Ruta.cs / RutaService for the draft-then-generate lifecycle).
-- ============================================================================

create table if not exists rutas (
    id                    serial primary key,
    usuario_id            uuid not null references usuarios (id) on delete cascade,
    tiempo_estimado_min   integer,
    fecha_generacion      timestamptz
);

create index if not exists ix_rutas_usuario_fecha_generacion on rutas (usuario_id, fecha_generacion);

create table if not exists ruta_lugares (
    id            serial primary key,
    ruta_id       integer not null references rutas (id) on delete cascade,
    lugar_id      integer not null references lugares (id) on delete restrict,
    orden_optimo  integer not null,
    unique (ruta_id, lugar_id)
);

-- ============================================================================
-- Reportes
-- ============================================================================

create table if not exists reportes_generados (
    id                serial primary key,
    admin_id          uuid not null references usuarios (id) on delete restrict,
    tipo_reporte      tipo_reporte not null,
    fecha_inicio      date not null,
    fecha_fin         date not null,
    formato_salida    formato_salida not null,
    archivo_url       varchar(500),
    fecha_generacion  timestamptz not null default now()
);

create index if not exists ix_reportes_admin_id on reportes_generados (admin_id);

-- ============================================================================
-- Triggers
-- ============================================================================

-- Keeps fecha_actualizacion current on every UPDATE, independent of whether the app remembered to.
create or replace function set_fecha_actualizacion()
returns trigger as $$
begin
    new.fecha_actualizacion := now();
    return new;
end;
$$ language plpgsql;

drop trigger if exists trg_usuarios_fecha_actualizacion on usuarios;
create trigger trg_usuarios_fecha_actualizacion
    before update on usuarios
    for each row execute function set_fecha_actualizacion();

drop trigger if exists trg_lugares_fecha_actualizacion on lugares;
create trigger trg_lugares_fecha_actualizacion
    before update on lugares
    for each row execute function set_fecha_actualizacion();

drop trigger if exists trg_usuario_configuracion_fecha_actualizacion on usuario_configuracion;
create trigger trg_usuario_configuracion_fecha_actualizacion
    before update on usuario_configuracion
    for each row execute function set_fecha_actualizacion();

-- Recomputes lugares.calificacion_promedio / total_calificaciones from *approved* reviews only,
-- so moderation decisions and the visible rating can never drift out of sync.
create or replace function actualizar_calificacion_lugar()
returns trigger as $$
declare
    target_lugar_id integer;
begin
    target_lugar_id := coalesce(new.lugar_id, old.lugar_id);

    update lugares
    set calificacion_promedio = coalesce((
            select round(avg(puntuacion)::numeric, 2)
            from calificaciones
            where lugar_id = target_lugar_id and estado_moderacion = 'aprobada'
        ), 0),
        total_calificaciones = (
            select count(*)
            from calificaciones
            where lugar_id = target_lugar_id and estado_moderacion = 'aprobada'
        )
    where id = target_lugar_id;

    return coalesce(new, old);
end;
$$ language plpgsql;

drop trigger if exists trg_calificaciones_actualizar_lugar on calificaciones;
create trigger trg_calificaciones_actualizar_lugar
    after insert or update or delete on calificaciones
    for each row execute function actualizar_calificacion_lugar();

-- Provisions the public.usuarios profile the moment a Supabase auth user is created. The API's
-- UsuarioProvisioner (Infrastructure/Auth/UsuarioProvisioner.cs) does the same thing lazily on
-- first authenticated request, as a fallback for projects where this trigger isn't installed —
-- the two are intentionally redundant, not either/or.
create or replace function handle_new_user()
returns trigger as $$
declare
    rol_turista_id integer;
begin
    select id into rol_turista_id from roles where nombre = 'turista' limit 1;
    if rol_turista_id is null then
        -- Seed data not loaded yet (run 002_seed.sql) — skip silently, UsuarioProvisioner covers it.
        return new;
    end if;

    insert into usuarios (id, rol_id, nombre_completo, correo_electronico, activo, fecha_registro, fecha_actualizacion)
    values (
        new.id,
        rol_turista_id,
        coalesce(new.raw_user_meta_data ->> 'full_name', split_part(new.email, '@', 1), 'Usuario ColombiaGO'),
        new.email,
        true,
        now(),
        now()
    )
    on conflict (id) do nothing;

    return new;
end;
$$ language plpgsql security definer set search_path = public;

drop trigger if exists trg_auth_handle_new_user on auth.users;
create trigger trg_auth_handle_new_user
    after insert on auth.users
    for each row execute function handle_new_user();
