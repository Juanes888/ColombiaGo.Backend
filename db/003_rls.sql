-- ColombiaGO — Row Level Security policies
--
-- The API itself connects with a direct Postgres connection string and therefore bypasses RLS
-- entirely (it does its own authorization in Application services / controller policies). These
-- policies are a second, independent layer: Supabase auto-exposes every public table over its
-- REST and GraphQL APIs, so if a client ever queries Postgrest directly with the anon/authenticated
-- key — bypassing this .NET API — RLS is what stops it from reading someone else's favorites or
-- writing into someone else's profile. Optional but recommended; skip only if you disable
-- Supabase's auto-generated data APIs entirely.
--
-- auth.uid() returns the current request's Supabase user id (null when unauthenticated).
-- Run after 001_schema.sql and 002_seed.sql.

-- ============================================================================
-- Enable RLS everywhere
-- ============================================================================

alter table roles                  enable row level security;
alter table categorias              enable row level security;
alter table logros                  enable row level security;
alter table usuarios                enable row level security;
alter table usuario_preferencias    enable row level security;
alter table usuario_configuracion   enable row level security;
alter table guias                   enable row level security;
alter table usuario_logros          enable row level security;
alter table tipos_atributo          enable row level security;
alter table caracteristicas         enable row level security;
alter table lugares                 enable row level security;
alter table lugar_atributos         enable row level security;
alter table lugar_caracteristicas   enable row level security;
alter table galeria_fotos           enable row level security;
alter table eventos                 enable row level security;
alter table calificaciones          enable row level security;
alter table favoritos_lugares       enable row level security;
alter table favoritos_eventos       enable row level security;
alter table historial_interacciones enable row level security;
alter table recomendaciones         enable row level security;
alter table rutas                   enable row level security;
alter table ruta_lugares            enable row level security;
alter table reportes_generados      enable row level security;

-- ============================================================================
-- Public read-only reference/catalog data
-- ============================================================================

drop policy if exists "roles_select_all" on roles;
create policy "roles_select_all" on roles for select using (true);

drop policy if exists "categorias_select_all" on categorias;
create policy "categorias_select_all" on categorias for select using (true);

drop policy if exists "logros_select_all" on logros;
create policy "logros_select_all" on logros for select using (true);

drop policy if exists "tipos_atributo_select_all" on tipos_atributo;
create policy "tipos_atributo_select_all" on tipos_atributo for select using (true);

drop policy if exists "caracteristicas_select_all" on caracteristicas;
create policy "caracteristicas_select_all" on caracteristicas for select using (true);

drop policy if exists "guias_select_all" on guias;
create policy "guias_select_all" on guias for select using (true);

-- ============================================================================
-- Lugares and their satellite tables — public can only see approved places;
-- everything else about a place inherits that same visibility.
-- ============================================================================

drop policy if exists "lugares_select_aprobados" on lugares;
create policy "lugares_select_aprobados" on lugares
    for select using (estado_publicacion = 'aprobado');

drop policy if exists "lugar_atributos_select" on lugar_atributos;
create policy "lugar_atributos_select" on lugar_atributos
    for select using (exists (
        select 1 from lugares l where l.id = lugar_id and l.estado_publicacion = 'aprobado'
    ));

drop policy if exists "lugar_caracteristicas_select" on lugar_caracteristicas;
create policy "lugar_caracteristicas_select" on lugar_caracteristicas
    for select using (exists (
        select 1 from lugares l where l.id = lugar_id and l.estado_publicacion = 'aprobado'
    ));

drop policy if exists "galeria_fotos_select" on galeria_fotos;
create policy "galeria_fotos_select" on galeria_fotos
    for select using (exists (
        select 1 from lugares l where l.id = lugar_id and l.estado_publicacion = 'aprobado'
    ));

drop policy if exists "eventos_select_all" on eventos;
create policy "eventos_select_all" on eventos for select using (true);

-- ============================================================================
-- Calificaciones — anyone can read approved reviews; a user can read/write their own
-- (regardless of moderation state, so they can see their own pending review).
-- ============================================================================

drop policy if exists "calificaciones_select_aprobadas" on calificaciones;
create policy "calificaciones_select_aprobadas" on calificaciones
    for select using (estado_moderacion = 'aprobada' or usuario_id = auth.uid());

drop policy if exists "calificaciones_insert_propia" on calificaciones;
create policy "calificaciones_insert_propia" on calificaciones
    for insert with check (usuario_id = auth.uid());

-- ============================================================================
-- Strictly per-user tables: a user may only see/modify their own rows.
-- ============================================================================

drop policy if exists "usuarios_select_propio" on usuarios;
create policy "usuarios_select_propio" on usuarios for select using (id = auth.uid());

drop policy if exists "usuarios_update_propio" on usuarios;
create policy "usuarios_update_propio" on usuarios for update using (id = auth.uid());

drop policy if exists "usuario_preferencias_all_propia" on usuario_preferencias;
create policy "usuario_preferencias_all_propia" on usuario_preferencias
    for all using (usuario_id = auth.uid()) with check (usuario_id = auth.uid());

drop policy if exists "usuario_configuracion_all_propia" on usuario_configuracion;
create policy "usuario_configuracion_all_propia" on usuario_configuracion
    for all using (id = auth.uid()) with check (id = auth.uid());

drop policy if exists "usuario_logros_select_propia" on usuario_logros;
create policy "usuario_logros_select_propia" on usuario_logros
    for select using (usuario_id = auth.uid());

drop policy if exists "favoritos_lugares_all_propia" on favoritos_lugares;
create policy "favoritos_lugares_all_propia" on favoritos_lugares
    for all using (usuario_id = auth.uid()) with check (usuario_id = auth.uid());

drop policy if exists "favoritos_eventos_all_propia" on favoritos_eventos;
create policy "favoritos_eventos_all_propia" on favoritos_eventos
    for all using (usuario_id = auth.uid()) with check (usuario_id = auth.uid());

drop policy if exists "historial_insert_propia" on historial_interacciones;
create policy "historial_insert_propia" on historial_interacciones
    for insert with check (usuario_id = auth.uid());

drop policy if exists "historial_select_propia" on historial_interacciones;
create policy "historial_select_propia" on historial_interacciones
    for select using (usuario_id = auth.uid());

drop policy if exists "recomendaciones_select_propia" on recomendaciones;
create policy "recomendaciones_select_propia" on recomendaciones
    for select using (usuario_id = auth.uid());

drop policy if exists "rutas_all_propia" on rutas;
create policy "rutas_all_propia" on rutas
    for all using (usuario_id = auth.uid()) with check (usuario_id = auth.uid());

drop policy if exists "ruta_lugares_all_propia" on ruta_lugares;
create policy "ruta_lugares_all_propia" on ruta_lugares
    for all using (exists (
        select 1 from rutas r where r.id = ruta_id and r.usuario_id = auth.uid()
    ));

-- ============================================================================
-- Admin-only: nothing here is exposed to anon/authenticated roles at all.
-- (No policy = no access once RLS is enabled; reportes_generados intentionally has none.)
-- ============================================================================
