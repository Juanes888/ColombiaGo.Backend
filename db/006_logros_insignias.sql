-- ============================================================================
-- 006 — Logros con insignias reales
--
-- Reemplaza los 3 logros del mock por 10 con su insignia (mobile/assets/badges/*.png) y agrega
-- las reglas para desbloquearlos de verdad. `icono` = 'badge:<nombre>' (el cliente elige la imagen);
-- `criterio` identifica la regla que evalúa sincronizar_logros().
-- Los ids 1-10 siguen el orden de la hoja de insignias. Idempotente.
-- ============================================================================

insert into logros (id, titulo, descripcion, icono, tono, criterio) values
    (1,  'Bienvenido a bordo',    'Completaste el recorrido de bienvenida.',        'badge:globe',    'blue',   'onboarding'),
    (2,  'Cartógrafo',            'Guardaste 3 o más paradas en tu ruta.',          'badge:map',      'green',  'ruta>=3'),
    (3,  'Brújula lista',         'Marcaste tu primer lugar favorito.',             'badge:compass',  'yellow', 'favoritos>=1'),
    (4,  'Pasaporte sellado',     'Ya tienes 5 lugares favoritos.',                 'badge:passport', 'purple', 'favoritos>=5'),
    (5,  'Ojo de fotógrafo',      'Dejaste tu primera reseña.',                     'badge:camera',   'red',    'resenas>=1'),
    (6,  'Cazador de miradores',  'Guardaste 3 lugares de naturaleza.',             'badge:mountain', 'green',  'naturaleza>=3'),
    (7,  'Amante de la cultura',  'Guardaste 3 lugares de cultura.',                'badge:church',   'yellow', 'cultura>=3'),
    (8,  'Vacaciones soñadas',    'Creaste tu primera guía de viaje con IA.',       'badge:beach',    'blue',   'guias>=1'),
    (9,  'Viajero frecuente',     'Creaste 3 guías de viaje con IA.',               'badge:plane',    'purple', 'guias>=3'),
    (10, 'Leyenda ColombiaGO',    'Desbloqueaste todas las demás insignias.',       'badge:trophy',   'red',    'todos')
on conflict (id) do update set
    titulo = excluded.titulo, descripcion = excluded.descripcion, icono = excluded.icono,
    tono = excluded.tono, criterio = excluded.criterio;

-- El serial queda por encima de los ids fijos para futuros logros.
select setval(pg_get_serial_sequence('logros', 'id'), greatest((select max(id) from logros), 10));

-- Evalúa las reglas para un usuario y registra los logros que ya cumple (no quita ninguno).
create or replace function sincronizar_logros(p_usuario uuid) returns void
language plpgsql
set search_path = public
as $$
declare
    v_onb    boolean;
    v_fav    integer;
    v_ruta   integer;
    v_res    integer;
    v_guias  integer;
    v_nat    integer;
    v_cul    integer;
begin
    select coalesce(onboarding_completado, false) into v_onb
      from usuario_configuracion where id = p_usuario;
    v_onb := coalesce(v_onb, false);

    select count(*) into v_fav from favoritos_lugares where usuario_id = p_usuario;

    select coalesce(max(c), 0) into v_ruta
      from (select count(*) as c
              from ruta_lugares rl join rutas r on r.id = rl.ruta_id
             where r.usuario_id = p_usuario
             group by r.id) x;

    select count(*) into v_res from calificaciones where usuario_id = p_usuario;
    select count(*) into v_guias from guias_viaje where usuario_id = p_usuario;

    select count(*) into v_nat
      from favoritos_lugares f
      join lugares l on l.id = f.lugar_id
      join categorias c on c.id = l.categoria_id
     where f.usuario_id = p_usuario and c.slug = 'nature';

    select count(*) into v_cul
      from favoritos_lugares f
      join lugares l on l.id = f.lugar_id
      join categorias c on c.id = l.categoria_id
     where f.usuario_id = p_usuario and c.slug = 'culture';

    insert into usuario_logros (usuario_id, logro_id)
    select p_usuario, l.id
      from logros l
     where (l.criterio = 'onboarding'     and v_onb)
        or (l.criterio = 'ruta>=3'        and v_ruta >= 3)
        or (l.criterio = 'favoritos>=1'   and v_fav >= 1)
        or (l.criterio = 'favoritos>=5'   and v_fav >= 5)
        or (l.criterio = 'resenas>=1'     and v_res >= 1)
        or (l.criterio = 'naturaleza>=3'  and v_nat >= 3)
        or (l.criterio = 'cultura>=3'     and v_cul >= 3)
        or (l.criterio = 'guias>=1'       and v_guias >= 1)
        or (l.criterio = 'guias>=3'       and v_guias >= 3)
    on conflict (usuario_id, logro_id) do nothing;

    -- La copa: todas las demás insignias.
    insert into usuario_logros (usuario_id, logro_id)
    select p_usuario, l.id
      from logros l
     where l.criterio = 'todos'
       and (select count(*) from usuario_logros ul join logros x on x.id = ul.logro_id
             where ul.usuario_id = p_usuario and x.criterio <> 'todos')
         >= (select count(*) from logros where criterio <> 'todos')
    on conflict (usuario_id, logro_id) do nothing;
end;
$$;

-- Solo el backend (rol postgres) la llama; no se expone por la API de datos de Supabase.
revoke all on function sincronizar_logros(uuid) from public, anon, authenticated;
