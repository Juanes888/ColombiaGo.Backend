-- ============================================================================
-- 008 · Roles por tipo de cuenta: turista, guía y negocio (+ admin)
--
-- * Nuevo rol "negocio": cuentas que publican lugares (restaurantes, hoteles, tours…). Lo que publican queda en
--   estado 'pendiente' hasta que un admin lo aprueba (la moderación ya existía en /api/v1/admin/lugares).
-- * Al registrarse, la app manda user_metadata.tipo_cuenta = turista | guia | negocio. El trigger crea el usuario con
--   ese rol. "admin" nunca se puede elegir desde la app: se asigna a mano en la base.
-- * Bucket público "fotos" para las fotos que suben guías y negocios; cada cuenta solo escribe en su carpeta
--   (<id del usuario>/archivo.jpg).
-- ============================================================================

insert into roles (nombre, descripcion) values
    ('negocio', 'Negocio que publica lugares (restaurantes, hoteles, tours…) sujetos a moderación')
on conflict (nombre) do nothing;

create or replace function handle_new_user() returns trigger as $$
declare
    tipo_meta   text;
    rol_id_new  integer;
    idioma_meta text;
begin
    tipo_meta := lower(coalesce(new.raw_user_meta_data ->> 'tipo_cuenta', 'turista'));
    if tipo_meta not in ('turista', 'guia', 'negocio') then tipo_meta := 'turista'; end if;

    select id into rol_id_new from roles where nombre = tipo_meta limit 1;
    if rol_id_new is null then
        select id into rol_id_new from roles where nombre = 'turista' limit 1;
    end if;
    if rol_id_new is null then return new; end if;

    insert into usuarios (id, rol_id, nombre_completo, correo_electronico, activo, fecha_registro, fecha_actualizacion)
    values (new.id, rol_id_new,
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

-- Fotos subidas desde la app -----------------------------------------------------------------------
insert into storage.buckets (id, name, public)
values ('fotos', 'fotos', true)
on conflict (id) do nothing;

drop policy if exists "fotos_insert_propia_carpeta" on storage.objects;
create policy "fotos_insert_propia_carpeta" on storage.objects
    for insert to authenticated
    with check (bucket_id = 'fotos' and (storage.foldername(name))[1] = (select auth.uid())::text);

drop policy if exists "fotos_update_propia_carpeta" on storage.objects;
create policy "fotos_update_propia_carpeta" on storage.objects
    for update to authenticated
    using (bucket_id = 'fotos' and (storage.foldername(name))[1] = (select auth.uid())::text);

drop policy if exists "fotos_delete_propia_carpeta" on storage.objects;
create policy "fotos_delete_propia_carpeta" on storage.objects
    for delete to authenticated
    using (bucket_id = 'fotos' and (storage.foldername(name))[1] = (select auth.uid())::text);
