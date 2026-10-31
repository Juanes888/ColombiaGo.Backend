-- ColombiaGO — 004: idioma, onboarding, traducciones en caché, guías de viaje con IA
-- y más lugares de Medellín (la guía IA solo usa lugares reales de la BD).
-- Idempotente. Ejecutar después de 001/002/003.

-- Preferencias de usuario ---------------------------------------------------
alter table usuario_configuracion
    add column if not exists idioma varchar(5) not null default 'es',
    add column if not exists onboarding_completado boolean not null default false;

do $$ begin
    alter table usuario_configuracion
        add constraint ck_usuario_configuracion_idioma check (idioma in ('es','en','pt','zh','fr'));
exception when duplicate_object then null; end $$;

-- Caché de traducciones del contenido de lugares ---------------------------
create table if not exists lugar_traducciones (
    id                serial primary key,
    lugar_id          integer not null references lugares (id) on delete cascade,
    idioma            varchar(5) not null check (idioma in ('en','pt','zh','fr')),
    nombre            varchar(300) not null,
    descripcion       varchar(4000) not null,
    eslogan           varchar(300),
    como_llegar       varchar(300),
    horario_atencion  varchar(300),
    fecha_creacion    timestamptz not null default now(),
    unique (lugar_id, idioma)
);

-- Guías de viaje generadas con IA ------------------------------------------
create table if not exists guias_viaje (
    id              serial primary key,
    usuario_id      uuid not null references usuarios (id) on delete cascade,
    ciudad          varchar(120) not null,
    dias            integer not null check (dias between 1 and 14),
    presupuesto     numeric(14, 2) not null check (presupuesto > 0),
    intereses       text[] not null default '{}',
    idioma          varchar(5) not null default 'es',
    contenido       jsonb not null,
    fecha_creacion  timestamptz not null default now()
);
create index if not exists ix_guias_viaje_usuario_fecha on guias_viaje (usuario_id, fecha_creacion desc);

-- Alta de usuario: perfil + configuración con el idioma elegido en el registro --
create or replace function handle_new_user() returns trigger as $$
declare
    rol_turista_id integer;
    idioma_meta    text;
begin
    select id into rol_turista_id from roles where nombre = 'turista' limit 1;
    if rol_turista_id is null then return new; end if;

    insert into usuarios (id, rol_id, nombre_completo, correo_electronico, activo, fecha_registro, fecha_actualizacion)
    values (new.id, rol_turista_id,
            coalesce(new.raw_user_meta_data ->> 'full_name', split_part(new.email, '@', 1), 'Usuario ColombiaGO'),
            new.email, true, now(), now())
    on conflict (id) do nothing;

    idioma_meta := coalesce(new.raw_user_meta_data ->> 'idioma', 'es');
    if idioma_meta not in ('es','en','pt','zh','fr') then idioma_meta := 'es'; end if;

    insert into usuario_configuracion (id, idioma)
    values (new.id, idioma_meta)
    on conflict (id) do nothing;

    return new;
end;
$$ language plpgsql security definer set search_path = public;

-- RLS -----------------------------------------------------------------------
alter table lugar_traducciones enable row level security;
alter table guias_viaje        enable row level security;

drop policy if exists "lugar_traducciones_select_all" on lugar_traducciones;
create policy "lugar_traducciones_select_all" on lugar_traducciones for select using (true);

drop policy if exists "guias_viaje_all_propia" on guias_viaje;
create policy "guias_viaje_all_propia" on guias_viaje
    for all using (usuario_id = (select auth.uid())) with check (usuario_id = (select auth.uid()));

-- Más lugares de Medellín (coordenadas aproximadas; rating 0 = sin reseñas todavía) -----
insert into lugares (categoria_id, registrado_por, nombre, slug, ciudad, region, descripcion, eslogan, direccion, latitud, longitud, horario_atencion, como_llegar, imagen_portada_url, rango_precio, costo_entrada, estado_publicacion) values
((select id from categorias where slug='culture'),null,'Parque Explora','parque-explora','Medellín','Antioquia','Museo interactivo de ciencia con acuario, vivario y planetario. Plan perfecto para familias y curiosos.','Ciencia que se toca y se vive','Cra. 52 #73-75, Medellín',6.271300,-75.565600,'9am - 5pm (cierra lunes)','Metro estación Universidad','/images/medellin.jpg','economico',35000,'aprobado'),
((select id from categorias where slug='nature'),null,'Jardín Botánico de Medellín','jardin-botanico-medellin','Medellín','Antioquia','Oasis verde en el centro de la ciudad con orquideorama, mariposario y una gran colección de plantas.','Un bosque dentro de la ciudad','Cl. 73 #51D-14, Medellín',6.271800,-75.563600,'9am - 5pm','Metro estación Universidad','/images/medellin.jpg','gratis',null,'aprobado'),
((select id from categorias where slug='culture'),null,'Plaza Botero','plaza-botero','Medellín','Antioquia','Plaza con 23 esculturas monumentales de Fernando Botero, frente al Museo de Antioquia.','Las esculturas de Botero al aire libre','Cra. 52 #52-43, La Candelaria, Medellín',6.251900,-75.568600,'Todo el día','Metro estación Parque Berrío','/images/comuna13.jpg','gratis',null,'aprobado'),
((select id from categorias where slug='nightlife'),null,'Parque Lleras','parque-lleras','Medellín','Antioquia','Corazón de la vida nocturna de El Poblado: bares, restaurantes y música hasta la madrugada.','La rumba de El Poblado','Cl. 10 #38, El Poblado, Medellín',6.208500,-75.569200,'6pm - 3am','Metro estación Poblado + taxi','/images/cali.jpg','moderado',null,'aprobado'),
((select id from categorias where slug='food'),null,'Mercado del Río','mercado-del-rio','Medellín','Antioquia','Mercado gastronómico con decenas de cocinas locales e internacionales, café de especialidad y cervezas artesanales.','Todo el sabor de Medellín bajo un techo','Cra. 48 #20-61, Medellín',6.237700,-75.576900,'11am - 10pm','Metro estación Industriales + taxi','/images/medellin.jpg','moderado',null,'aprobado'),
((select id from categorias where slug='culture'),null,'Museo de Arte Moderno de Medellín','museo-arte-moderno-medellin','Medellín','Antioquia','Arte contemporáneo colombiano e internacional en una antigua fábrica reformada.','Arte contemporáneo en Ciudad del Río','Cra. 44 #19A-100, Medellín',6.231800,-75.575100,'9am - 6pm (cierra domingo)','Metro estación Industriales','/images/comuna13.jpg','economico',12000,'aprobado'),
((select id from categorias where slug='food'),null,'Laureles: café y gastronomía','laureles','Medellín','Antioquia','Barrio arbolado y caminable con cafés de especialidad, panaderías y restaurantes sobre la Primera Parroquia.','El barrio más tranquilo para comer bien','Barrio Laureles, Medellín',6.244200,-75.592000,'Todo el día','Metro estación Estadio + caminata','/images/medellin.jpg','moderado',null,'aprobado'),
((select id from categorias where slug='culture'),null,'Santa Elena y los silleteros','santa-elena-silleteros','Medellín','Antioquia','Vereda de tradición floricultora, origen del Desfile de Silleteros. Fincas, flores y gastronomía campesina.','Flores y tradición campesina','Santa Elena, Medellín',6.214500,-75.492900,'8am - 5pm','Metro + Metrocable Arví + bus','/images/medellin.jpg','economico',20000,'aprobado'),
((select id from categorias where slug='nature'),null,'Cerro El Volador','cerro-el-volador','Medellín','Antioquia','Pulmón verde con senderos, miradores y vista de 360° sobre el Valle de Aburrá.','Mirador natural en plena ciudad','Cl. 65 #85-70, Medellín',6.264600,-75.583800,'6am - 6pm','Metro estación Universidad + caminata','/images/medellin.jpg','gratis',null,'aprobado'),
((select id from categorias where slug='culture'),null,'Parque de los Pies Descalzos','parque-pies-descalzos','Medellín','Antioquia','Parque sensorial con arena, agua y bosque de bambú para caminar descalzo, junto al edificio EPM.','Camina descalzo en el centro','Cra. 57 #42-139, Medellín',6.243600,-75.574400,'8am - 8pm','Metro estación Alpujarra','/images/medellin.jpg','gratis',null,'aprobado')
on conflict (slug) do nothing;
