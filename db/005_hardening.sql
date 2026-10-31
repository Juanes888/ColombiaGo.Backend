-- ColombiaGO — 005: endurecimiento detectado por get_advisors de Supabase.
-- handle_new_user() solo debe dispararla el trigger de auth.users, nunca /rest/v1/rpc.
revoke execute on function public.handle_new_user() from public, anon, authenticated;
-- Las extensiones no deben vivir en `public` (los índices GIN existentes siguen funcionando).
create schema if not exists extensions;
alter extension pg_trgm set schema extensions;
-- Funciones de trigger con search_path fijo (aviso function_search_path_mutable).
alter function public.set_fecha_actualizacion() set search_path = public;
alter function public.actualizar_calificacion_lugar() set search_path = public;
