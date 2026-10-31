-- ColombiaGO — seed data
--
-- Loads: roles, the 4 categories + their tones/icons, 3 achievements, the 18 destinations and
-- 4 guides that already exist as hardcoded mock data in the frontend
-- (src/data/destinations.ts, src/data/guides.ts, ProfilePage.tsx's ACHIEVEMENTS) — so the app
-- has real content to show from the very first request instead of an empty database.
--
-- Run after 001_schema.sql. Idempotent: every insert is ON CONFLICT DO NOTHING/UPDATE keyed on
-- a natural unique column (slug, nombre), so re-running this file is safe.

-- ============================================================================
-- Roles
-- ============================================================================

insert into roles (nombre, descripcion) values
    ('turista', 'Usuario que explora, guarda favoritos y arma rutas'),
    ('guia', 'Guía turístico verificado'),
    ('admin', 'Administrador con acceso a moderación y reportes')
on conflict (nombre) do nothing;

-- ============================================================================
-- Categorías (HOME_CATEGORIES / CATEGORIES in src/data/destinations.ts)
-- ============================================================================

insert into categorias (nombre, slug, icono, tono, activa) values
    ('Gastronomía',    'food',      'utensils',   'yellow', true),
    ('Cultura',        'culture',   'landmark',   'blue',   true),
    ('Naturaleza',     'nature',    'mountain',   'green',  true),
    ('Vida nocturna',  'nightlife', 'moon',       'red',    true)
on conflict (slug) do update set
    nombre = excluded.nombre,
    icono  = excluded.icono,
    tono   = excluded.tono,
    activa = excluded.activa;

-- ============================================================================
-- Logros (ACHIEVEMENTS in ProfilePage.tsx)
-- ============================================================================

insert into logros (titulo, descripcion, icono, tono) values
    ('Cazador de miradores', 'Descubriste 10+ miradores increíbles.', '🏔️', 'green'),
    ('Noche loca',           'Estuviste en 5+ bares o discotecas.',    '🪩', 'yellow'),
    ('Amigo del Metrocable',  'Usaste 5 líneas del Metrocable.',       '🚡', 'blue')
on conflict do nothing;

-- ============================================================================
-- Lugares (DESTINATIONS in src/data/destinations.ts)
--
-- rango_precio is inferred from each place's mock "price" string (a free-text label there,
-- an enum bucket here — see DisplayFormatting.ToPriceLabel for how the API turns it back into
-- a display string). costo_entrada holds the numeric floor for "Desde $X" places; null otherwise.
-- registrado_por is left null (system-provided content, not a community submission).
--
-- calificacion_promedio/total_calificaciones are seeded directly from the mock's rating/reviews
-- fields (e.g. "2k reseñas" -> 2000) rather than left at zero: normally
-- trg_calificaciones_actualizar_lugar keeps these in sync from real calificaciones rows, but
-- since no reviews are seeded here, setting them explicitly is what makes the app look like a
-- populated product on first launch instead of eighteen freshly-created, unrated places. A real
-- review submitted later recalculates both from scratch via that trigger, overwriting these.
-- ============================================================================

insert into lugares (
    categoria_id, registrado_por, nombre, slug, ciudad, region, descripcion, eslogan, direccion,
    latitud, longitud, horario_atencion, como_llegar, imagen_portada_url,
    calificacion_promedio, total_calificaciones,
    rango_precio, costo_entrada, destacado, estado_publicacion, fecha_registro, fecha_actualizacion
) values
    ((select id from categorias where slug = 'culture'), null,
     'Comuna 13: Arte y Transformación', 'comuna-13', 'Medellín', 'Antioquia',
     'Descubre la historia de resiliencia y el arte urbano más vibrante de Medellín. Graffiti, escaleras eléctricas y una comunidad que reescribió su destino.',
     'Arte, historia y transformación', 'Comuna 13, San Javier, Medellín, Antioquia',
     6.256400, -75.612500, '8am - 6pm', 'Metro + Metrocable', '/images/comuna13.jpg',
     4.9, 2000, 'gratis', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nature'), null,
     'Parque Arví', 'parque-arvi', 'Medellín', 'Antioquia',
     'Escápate del ritmo de la ciudad entre bosques nativos, mercados campesinos y senderos con vistas increíbles del Valle de Aburrá.',
     'Bosque, senderos y aire de montaña', 'Parque Arví, Santa Elena, Medellín, Antioquia',
     6.269800, -75.503700, '9am - 5pm', 'Metro + Metrocable', '/images/medellin.jpg',
     4.8, 1400, 'economico', 15000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Pueblito Paisa', 'pueblito-paisa', 'Medellín', 'Antioquia',
     'Una pequeña plaza tradicional en lo alto del Cerro Nutibara, perfecta para conocer la arquitectura paisa y ver Medellín desde arriba.',
     'Tradición paisa con vista a la ciudad', 'Cerro Nutibara, Medellín, Antioquia',
     6.238800, -75.585500, '8am - 10pm', 'Metro + taxi', '/images/medellin.jpg',
     4.7, 980, 'gratis', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Museo de Antioquia', 'museo-de-antioquia', 'Medellín', 'Antioquia',
     'Recorre colecciones de arte colombiano y latinoamericano frente a la Plaza Botero, en pleno centro histórico de Medellín.',
     'Arte colombiano en el corazón de Medellín', 'Plaza Botero, La Candelaria, Medellín, Antioquia',
     6.252800, -75.569200, '10am - 5:30pm', 'Metro o tranvía', '/images/comuna13.jpg',
     4.7, 760, 'economico', 15000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nightlife'), null,
     'Provenza', 'provenza', 'Medellín', 'Antioquia',
     'Una zona llena de restaurantes, cafés, bares y música para descubrir el lado más cosmopolita y alegre de Medellín.',
     'Sabores, música y noches vibrantes', 'El Poblado, Medellín, Antioquia',
     6.209700, -75.567600, '5pm - 2am', 'Metro + taxi', '/images/cali.jpg',
     4.6, 1200, 'moderado', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nature'), null,
     'Guatapé', 'guatape', 'A 2h de Medellín', 'Antioquia',
     'El Peñol, zócalos de colores y una laguna que parece infinita. El paseo clásico desde Medellín para respirar montaña.',
     'Colores, paisajes y magia', 'Guatapé, Antioquia',
     6.232400, -75.159700, '6am - 6pm', 'Bus o tour', '/images/guatape.jpg',
     4.9, 1800, 'economico', 25000, true, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Cartagena', 'cartagena', 'Cartagena', 'Bolívar',
     'Ciudad amurallada Patrimonio de la Humanidad, con callejones coloniales, balcones floridos y atardeceres sobre el Caribe.',
     'Murallas, color y brisa caribeña', 'Centro Histórico, Cartagena, Bolívar',
     10.423000, -75.548600, 'Todo el día', 'A pie o bicitaxi', '/images/cartagena.jpg',
     4.8, 3100, 'gratis', null, true, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'food'), null,
     'Salento', 'salento', 'Salento', 'Quindío',
     'Palmas de cera, fincas cafeteras y el Valle de Cocora: el paisaje más icónico del café colombiano.',
     'El corazón del Eje Cafetero', 'Salento, Quindío',
     4.636700, -75.570600, '7am - 5pm', 'Jeep Willys', '/images/salento.jpg',
     4.9, 980, 'costoso', 80000, true, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nature'), null,
     'Tayrona', 'tayrona', 'Santa Marta', 'Magdalena',
     'Playas salvajes, senderos entre ceibas y la cultura ancestral de la Sierra Nevada de Santa Marta.',
     'Selva que besa el Caribe', 'Parque Nacional Tayrona, Magdalena',
     11.316700, -74.033300, '8am - 4pm', 'Bus + caminata', '/images/tayrona.jpg',
     4.8, 1200, 'economico', 32000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nightlife'), null,
     'Cali', 'cali', 'Cali', 'Valle del Cauca',
     'Rumba, río y azúcar. Cali se vive de noche, se baila de día y se come con alma vallecaucana.',
     'Capital mundial de la salsa', 'Cali, Valle del Cauca',
     3.451600, -76.532000, '6pm - 3am', 'MIO o taxi', '/images/cali.jpg',
     4.6, 740, 'moderado', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Bogotá', 'bogota', 'Bogotá', 'Cundinamarca',
     'Museo del Oro, La Candelaria y una escena gastronómica que no para. La puerta de entrada a los Andes.',
     'Capital andina de arte y sabor', 'La Candelaria, Bogotá',
     4.598100, -74.075800, 'Según sitio', 'TransMilenio', '/images/bogota.jpg',
     4.6, 2400, 'moderado', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nature'), null,
     'San Andrés', 'san-andres', 'San Andrés', 'San Andrés',
     'Aguas turquesa, cultura raizal y el ritmo del calipso en el archipiélago más caribeño de Colombia.',
     'El mar de los siete colores', 'San Andrés Isla',
     12.554600, -81.718500, 'Todo el día', 'Moto o buggy', '/images/san-andres.jpg',
     4.7, 890, 'costoso', 90000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Villa de Leyva', 'villa-de-leyva', 'Villa de Leyva', 'Boyacá',
     'Camina por una de las plazas más grandes de América, descubre sus calles blancas y disfruta el paisaje de los Andes boyacenses.',
     'Historia, plazas y cielos de Boyacá', 'Villa de Leyva, Boyacá',
     5.633300, -73.525000, 'Todo el día', 'Bus o carro', '/images/villa.jpg',
     4.8, 1100, 'gratis', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nature'), null,
     'Jardín', 'jardin', 'Jardín', 'Antioquia',
     'Un pueblo colorido entre montañas, balcones floridos y caminos que llevan a cascadas, reservas naturales y fincas cafeteras.',
     'Montañas, colibríes y café', 'Jardín, Antioquia',
     5.598900, -75.818100, '7am - 6pm', 'Bus + caminata', '/images/salento.jpg',
     4.9, 680, 'economico', 35000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Barichara', 'barichara', 'Barichara', 'Santander',
     'Calles empedradas, arquitectura colonial y el Camino Real hacia Guane en uno de los pueblos más bonitos de Colombia.',
     'Piedra, calma y caminos reales', 'Barichara, Santander',
     6.634400, -73.161900, 'Todo el día', 'A pie o bus', '/images/villa.jpg',
     4.8, 540, 'gratis', null, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'culture'), null,
     'Santa Cruz de Mompox', 'mompox', 'Mompox', 'Bolívar',
     'Patios, iglesias y talleres de filigrana en una isla fluvial donde el tiempo pasa despacio y cada esquina cuenta una historia.',
     'Magia colonial a orillas del río', 'Santa Cruz de Mompox, Bolívar',
     9.241700, -74.428600, 'Todo el día', 'Bus + lancha', '/images/cartagena.jpg',
     4.7, 430, 'economico', 45000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'nature'), null,
     'Santa Marta', 'santa-marta', 'Santa Marta', 'Magdalena',
     'La ciudad más antigua de Colombia como punto de partida para playas, senderos de la Sierra Nevada y noches frente al Caribe.',
     'Mar, sierra y atardeceres dorados', 'Santa Marta, Magdalena',
     11.240400, -74.199000, 'Todo el día', 'Bus o taxi', '/images/tayrona.jpg',
     4.8, 1600, 'economico', 30000, false, 'aprobado', now(), now()),

    ((select id from categorias where slug = 'food'), null,
     'Popayán', 'popayan', 'Popayán', 'Cauca',
     'Arquitectura blanca, cocina tradicional y una Semana Santa llena de memoria en la elegante capital caucana.',
     'Sabores blancos del Cauca', 'Centro Histórico, Popayán, Cauca',
     2.444100, -76.614600, 'Según sitio', 'A pie o taxi', '/images/bogota.jpg',
     4.7, 390, 'moderado', null, false, 'aprobado', now(), now())
on conflict (slug) do nothing;

-- ============================================================================
-- Guías (GUIDES in src/data/guides.ts) — seeded as unlinked directory entries
-- (usuario_id null); see Guia.cs for why.
-- ============================================================================

insert into guias (
    usuario_id, nombre, foto_url, especialidad, etiqueta, tono_etiqueta, insignia, lugar,
    tarifa_hora, idiomas, destacado, verificado, calificacion_promedio, total_calificaciones, fecha_registro
) values
    (null, 'Mateo', '/images/guia-mateo.jpg', 'Experto en Café', 'Café', 'yellow', 'Experto en Café',
     'Salento, Quindío', 70000, array['ES', 'EN'], true, true, 5.0, 128, now()),

    (null, 'Camila', '/images/guide-camila.jpg', 'Arte Urbana', 'Arte Urbana', 'purple', null,
     'Medellín', 70000, array['ES', 'EN'], false, true, 4.9, 96, now()),

    (null, 'Andrés', '/images/guide-andres.jpg', 'Senderismo', 'Senderismo', 'green', null,
     'Cocora, Quindío', 60000, array['ES'], false, true, 4.8, 74, now()),

    (null, 'Lucía', '/images/guide-lucia.jpg', 'Cultura e Historia', 'Cultura & Historia', 'red', null,
     'Cartagena', 80000, array['ES', 'EN'], false, true, 4.9, 112, now())
on conflict do nothing;
