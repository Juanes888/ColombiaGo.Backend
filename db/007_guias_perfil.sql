-- ============================================================================
-- 007 · Perfil completo de los guías locales
--
-- La pantalla "Guías" ahora abre el detalle de cada guía: presentación, años de experiencia y tours realizados.
-- `descripcion` es el texto base en español; `descripcion_i18n` guarda el mismo texto en los demás idiomas
-- ({"en": "...", "pt": "...", "fr": "...", "zh": "..."}) para no depender de la IA en cada visita.
-- ============================================================================

alter table guias
    add column if not exists descripcion       text,
    add column if not exists descripcion_i18n  jsonb   not null default '{}'::jsonb,
    add column if not exists experiencia_anios integer not null default 0,
    add column if not exists tours_realizados  integer not null default 0;

update guias set
    descripcion = 'Crecí entre los cafetales de Salento. Te llevo a una finca familiar para que veas todo el proceso, del grano a la taza, y terminamos con una cata guiada. También recorremos el pueblo y sus miradores.',
    descripcion_i18n = jsonb_build_object(
        'en', 'I grew up among the coffee farms of Salento. I take you to a family farm so you can see the whole process, from bean to cup, and we finish with a guided tasting. We also walk through the town and its viewpoints.',
        'pt', 'Cresci entre os cafezais de Salento. Levo você a uma fazenda familiar para ver todo o processo, do grão à xícara, e terminamos com uma degustação guiada. Também percorremos a cidade e seus mirantes.',
        'fr', 'J''ai grandi parmi les plantations de café de Salento. Je vous emmène dans une ferme familiale pour voir tout le processus, du grain à la tasse, et nous terminons par une dégustation guidée. Nous visitons aussi le village et ses belvédères.',
        'zh', '我在萨连托的咖啡种植园中长大。我会带您参观一家家庭农场，了解从咖啡豆到杯中咖啡的全过程，最后进行导赏品鉴。我们还会游览小镇和观景台。'),
    experiencia_anios = 9, tours_realizados = 480
where nombre = 'Mateo';

update guias set
    descripcion = 'Artista y guía de la Comuna 13. Recorremos los grafitis, escuchamos la historia detrás de cada mural y conocemos a los artistas del barrio. Un paseo para entender cómo Medellín se transformó.',
    descripcion_i18n = jsonb_build_object(
        'en', 'Artist and guide in Comuna 13. We walk past the graffiti, hear the story behind each mural and meet the neighborhood''s artists. A tour to understand how Medellín transformed itself.',
        'pt', 'Artista e guia na Comuna 13. Percorremos os grafites, ouvimos a história por trás de cada mural e conhecemos os artistas do bairro. Um passeio para entender como Medellín se transformou.',
        'fr', 'Artiste et guide dans la Comuna 13. Nous parcourons les graffitis, écoutons l''histoire de chaque fresque et rencontrons les artistes du quartier. Une visite pour comprendre comment Medellín s''est transformée.',
        'zh', '我是第13区的艺术家兼向导。我们会走过涂鸦墙，聆听每幅壁画背后的故事，并结识社区里的艺术家。这是一次了解麦德林如何蜕变的旅程。'),
    experiencia_anios = 6, tours_realizados = 350
where nombre = 'Camila';

update guias set
    descripcion = 'Guía de montaña en el Valle de Cocora. Caminamos entre palmas de cera y bosque de niebla, con paradas para observar aves y tomar fotos. Voy a tu ritmo, con rutas para principiantes y para caminantes con experiencia.',
    descripcion_i18n = jsonb_build_object(
        'en', 'Mountain guide in the Cocora Valley. We hike among wax palms and cloud forest, with stops to watch birds and take photos. I go at your pace, with routes for beginners and experienced hikers.',
        'pt', 'Guia de montanha no Vale de Cocora. Caminhamos entre palmeiras-de-cera e floresta de neblina, com paradas para observar aves e tirar fotos. Vou no seu ritmo, com rotas para iniciantes e caminhantes experientes.',
        'fr', 'Guide de montagne dans la vallée de Cocora. Nous marchons parmi les palmiers à cire et la forêt de nuages, avec des arrêts pour observer les oiseaux et prendre des photos. Je m''adapte à votre rythme, avec des parcours pour débutants et randonneurs confirmés.',
        'zh', '我是科科拉山谷的登山向导。我们在蜡棕榈和云雾林间徒步，沿途停下观鸟、拍照。我会配合您的节奏，既有适合新手的路线，也有适合资深徒步者的路线。'),
    experiencia_anios = 8, tours_realizados = 410
where nombre = 'Andrés';

update guias set
    descripcion = 'Historiadora de Cartagena. Recorremos la Ciudad Amurallada, Getsemaní y las fortalezas contando la historia de la ciudad, sus leyendas y su cultura, con paradas para probar la comida local.',
    descripcion_i18n = jsonb_build_object(
        'en', 'Historian from Cartagena. We walk through the Walled City, Getsemaní and the fortresses, telling the story of the city, its legends and its culture, with stops to try local food.',
        'pt', 'Historiadora de Cartagena. Percorremos a Cidade Murada, Getsemaní e as fortalezas contando a história da cidade, suas lendas e sua cultura, com paradas para provar a comida local.',
        'fr', 'Historienne de Carthagène. Nous parcourons la ville fortifiée, Getsemaní et les forteresses en racontant l''histoire de la ville, ses légendes et sa culture, avec des arrêts pour goûter la cuisine locale.',
        'zh', '我是卡塔赫纳的历史学者。我们会游览老城、赫特塞马尼区和要塞，讲述这座城市的历史、传说与文化，并安排品尝当地美食。'),
    experiencia_anios = 11, tours_realizados = 620
where nombre = 'Lucía';
