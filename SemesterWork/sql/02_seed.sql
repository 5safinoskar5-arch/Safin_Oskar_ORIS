-- Тестовые данные (seed) для Star Wars: Unlimited DB
-- Учетные записи для входа (соль "swu_salt_2026:"):
--   admin  / Admin123!  (роль Admin)
--   player / Player123! (роль User)
-- Карты сгенерированы из реальных данных sw-unlimited-db.com (выпуск IC27, страницы 1-4).

INSERT INTO users (username, email, password_hash, role)
VALUES
    (
        'admin',
        'admin@swudb.local',
        '14961e846cdd166b0304b865d290fccbc2e734b9a597cd14ab1c7854de807dfb',
        'Admin'
    ),
    (
        'player',
        'player@swudb.local',
        '1e9ca1cfbdd4f0f62a5e67a1f1cce19475e5c7d8de6fe92590f6a1440c4101ce',
        'User'
    )
ON CONFLICT (username) DO NOTHING;

INSERT INTO sets (code, name, release_date, description)
VALUES
    (
        'IC27',
        'Icons 2027',
        '2026-11-20',
        'Юбилейный выпуск с культовыми героями, злодеями и ключевыми событиями всех эпох саги.'
    ),
    (
        'SOR',
        'Spark of Rebellion',
        '2024-03-08',
        'Первый базовый выпуск Star Wars: Unlimited, посвященный противостоянию Альянса повстанцев и Галактической Империи.'
    ),
    (
        'SHD',
        'Shadows of the Galaxy',
        '2024-07-12',
        'Второй выпуск с акцентом на Внешнее Кольцо, охотников за головами, контрабандистов и тайные операции.'
    )
ON CONFLICT (code) DO NOTHING;

INSERT INTO cards (
    set_id, card_number, name, subtitle, card_type, aspect, rarity,
    cost, power, hp, traits, ability_text, image_url, artist
)
VALUES
    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-001',
        'Darth Vader',
        'No One To Stop Us',
        'Leader',
        'Vigilance, Villainy',
        'Starter',
        7, 5, 7,
        'Force, Imperial, Sith',
        '<strong>On Attack:</strong> You may defeat another friendly unit. If you do, draw a card and heal 2 damage from your base.<br>If you control 7 or more resources, deploy this leader.',
        '/images/cards/darth-vader-no-one-to-stop-us.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-002',
        'Rey',
        'In Balance',
        'Leader',
        'Vigilance, Heroism',
        'Common',
        5, 3, 6,
        'Force, Resistance',
        '<strong>On Attack:</strong> You may give an Experience token to a damaged unit.<br>If you control 5 or more resources, deploy this leader.',
        '/images/cards/rey-in-balance.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-003',
        'Kylo Ren',
        'Supreme Leader',
        'Leader',
        'Command, Villainy',
        'Common',
        5, 5, 3,
        'Force, First Order, Official',
        '<strong>When Deployed:</strong> You may return a unit from your discard pile to your hand.<br>If you control 5 or more resources, deploy this leader.',
        '/images/cards/kylo-ren-supreme-leader.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-004',
        'Mace Windu',
        'Here With Backup',
        'Leader',
        'Command, Heroism',
        'Common',
        6, 4, 7,
        'Force, Jedi, Republic',
        '<strong>Restore 2</strong><br><strong>When Deployed:</strong> Resource the top card of your deck.<br>If you control 6 or more resources, deploy this leader.',
        '/images/cards/mace-windu-here-with-backup.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-005',
        'Darth Maul',
        'Mechanism Of Menace',
        'Leader',
        'Aggression, Villainy',
        'Common',
        5, 4, 5,
        'Force, Sith',
        '<strong>On Attack:</strong> You may deal 1 damage to a unit.<br>If you control 5 or more resources, deploy this leader.',
        '/images/cards/darth-maul-mechanism-of-menace.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-006',
        'Luke Skywalker',
        'I Have To Face Him',
        'Leader',
        'Aggression, Heroism',
        'Common',
        6, 4, 6,
        'Force, Jedi, Rebel',
        '<strong>Overwhelm</strong> (When attacking an enemy unit, deal excess damage to the opponent''s base.)<br><strong>On Attack:</strong> You may discard a card from your hand. If you do, this unit gets +2/+0 for this attack.<br>Draw 2 more cards in your starting hand.<br>If you control 6 or more resources, deploy this leader.',
        '/images/cards/luke-skywalker-i-have-to-face-him.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-007',
        'Asajj Ventress',
        'Bald And Beautiful',
        'Leader',
        'Cunning, Villainy',
        'Common',
        6, 5, 6,
        'Force, Separatist, Sith',
        '<strong>Saboteur</strong> (When this unit attacks, ignore Sentinel and defeat the defender''s Shields.)<br><strong>On Attack:</strong> You may ready a resource.<br>If you control 6 or more resources, deploy this leader.',
        '/images/cards/asajj-ventress-bald-and-beautiful.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-008',
        'Princess Leia',
        'On A Diplomatic Mission',
        'Leader',
        'Cunning, Heroism',
        'Starter',
        6, 4, 7,
        'Rebel, Official',
        '<strong>On Attack:</strong> Draw a card, then put a card from your hand on the top or bottom of your deck.<br>If you control 6 or more resources, deploy this leader.',
        '/images/cards/princess-leia-on-a-diplomatic-mission.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-009',
        'City in the Clouds',
        NULL,
        'Base',
        'Vigilance',
        'Common',
        0, NULL, 30,
        'Cloud City',
        NULL,
        '/images/cards/city-in-the-clouds.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-010',
        'Echo Base',
        NULL,
        'Base',
        'Command',
        'Common',
        0, NULL, 30,
        'Hoth',
        NULL,
        '/images/cards/echo-base.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-011',
        'KCM Mining Facility',
        NULL,
        'Base',
        'Aggression',
        'Common',
        0, NULL, 30,
        'Mustafar',
        NULL,
        '/images/cards/kcm-mining-facility.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-012',
        'Mos Eisley',
        NULL,
        'Base',
        'Cunning',
        'Common',
        0, NULL, 30,
        'Tatooine',
        NULL,
        '/images/cards/mos-eisley.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-013',
        'Nute Gunray',
        'Ring Ring, Space Calling',
        'Unit',
        'Vigilance, Villainy',
        'Rare',
        1, 1, 4,
        'Separatist, Official',
        'Each other friendly <strong>Separatist</strong> unit gains <strong>Restore 1.</strong> (When it attacks, heal 1 damage from your base.)',
        '/images/cards/nute-gunray-ring-ring-space-calling.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-014',
        'Dryden Vos',
        'Thumb-Spiking The Scale',
        'Unit',
        'Vigilance, Villainy',
        'Rare',
        2, 1, 4,
        'Underworld',
        '<strong>On Attack:</strong> You may deal 1 damage to a friendly unit. If it survives, give an Experience token to it.',
        '/images/cards/dryden-vos-thumb-spiking-the-scale.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-015',
        'Onyx Squadron Brute',
        NULL,
        'Unit',
        'Vigilance, Villainy',
        'Common',
        2, 2, 3,
        'Imperial, Vehicle, Fighter',
        '<strong>When Defeated:</strong> Heal 2 damage from a base.',
        '/images/cards/onyx-squadron-brute.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-016',
        'Rune Haako',
        'Scheming Second',
        'Unit',
        'Vigilance, Villainy',
        'Uncommon',
        2, 3, 2,
        'Separatist, Official',
        '<strong>When Played:</strong> If a friendly unit was defeated this phase, you may give a unit -1/-1 for this phase.',
        '/images/cards/rune-haako.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-017',
        'Death Trooper',
        NULL,
        'Unit',
        'Vigilance, Villainy',
        'Common',
        3, 3, 3,
        'Imperial, Trooper',
        '<strong>When Played:</strong> Deal 2 damage to a friendly ground unit and 2 damage to an enemy ground unit.',
        '/images/cards/death-trooper.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-018',
        'Magistrate''s Scout',
        NULL,
        'Unit',
        'Vigilance, Villainy',
        'Common',
        3, 2, 4,
        'Imperial, Trooper',
        '<strong>Restore 2</strong> (When this unit attacks, heal 2 damage from your base.)',
        '/images/cards/magistrates-scout.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-019',
        'Soulless One',
        'Soulless Fun',
        'Unit',
        'Vigilance, Villainy',
        'Legendary',
        4, 3, 5,
        'Separatist, Vehicle, Fighter',
        '<strong>When Played:</strong> Choose an enemy ground unit. While this unit is in play, the chosen unit gets -3/-0.',
        '/images/cards/soulless-one-soulless-fun.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-020',
        'Admiral Piett',
        'In Command Now',
        'Unit',
        'Vigilance, Villainy',
        'Starter',
        4, 2, 5,
        'Imperial, Official',
        '<strong>On Attack:</strong> If you control an Aggression unit, draw a card.',
        '/images/cards/admiral-piett.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-021',
        'General Grievous',
        'Four-Armed Menace',
        'Unit',
        'Vigilance, Villainy',
        'Legendary',
        4, 4, 5,
        'Separatist, Official',
        '<strong>When you play an upgrade on this unit:</strong> Choose one that hasn''t been chosen this phase:<br>- Give an Experience token to him.<br>- Heal 3 damage from your base.<br>- Draw a card.',
        '/images/cards/general-grievous-four-armed-menace.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-022',
        'Moff Gideon',
        'Cold Calling',
        'Unit',
        'Vigilance, Villainy',
        'Uncommon',
        5, 3, 6,
        'Imperial, Official',
        'If a friendly unit was defeated this phase, this unit costs 2 less to play.',
        '/images/cards/moff-gideon-cold-calling.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-023',
        'Grand Moff Tarkin',
        'Cracking Down On Lothal',
        'Unit',
        'Vigilance, Villainy',
        'Rare',
        5, 3, 6,
        'Imperial, Official',
        'Each friendly unit with <strong>Sentinel</strong> gets +1/+1.',
        '/images/cards/grand-moff-tarkin-cracking-down-on-lothal.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-024',
        'Grand Admiral Thrawn',
        'Listen To Me Carefully',
        'Unit',
        'Vigilance, Villainy',
        'Rare',
        6, 4, 4,
        'Imperial, Official',
        '<strong>When Played/On Attack/When Defeated:</strong> You may give an Experience token to another friendly unit. It gains <strong>Sentinel</strong> for this phase.',
        '/images/cards/grand-admiral-thrawn-listen-to-me-carefully.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-025',
        'Blizzard One',
        'Veers At The Helm',
        'Unit',
        'Vigilance, Villainy',
        'Starter',
        7, 5, 7,
        'Imperial, Vehicle, Walker',
        '<strong>When Played:</strong> You may defeat a non-leader ground unit with 3 or less remaining HP.',
        '/images/cards/blizzard-one.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-026',
        'Darth Sidious',
        'Move Against The Jedi',
        'Unit',
        'Vigilance, Villainy',
        'Rare',
        7, 5, 8,
        'Force, Separatist, Sith',
        '<strong>Restore 3</strong> (When this unit attacks, heal 3 damage from your base.)<br><strong>When you heal damage from your base:</strong> Deal that much damage to an enemy unit.',
        '/images/cards/darth-sidious-move-against-the-jedi.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-027',
        'AT-AT Suppressor',
        NULL,
        'Unit',
        'Vigilance, Villainy',
        'Uncommon',
        8, 8, 8,
        'Imperial, Vehicle, Walker',
        '<strong>When Played:</strong> Exhaust all ground units.',
        '/images/cards/at-at-suppressor.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-028',
        'Avenger',
        'Hunting The Rebels',
        'Unit',
        'Vigilance, Villainy',
        'Starter',
        8, 8, 6,
        'Imperial, Vehicle, Capital Ship',
        '<strong>When Played:</strong> Deal 1 damage to each other unit (including friendly units).',
        '/images/cards/avenger.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-029',
        'Chimaera',
        'Grand Admiral''s Might',
        'Unit',
        'Vigilance, Villainy',
        'Rare',
        10, 8, 8,
        'Imperial, Vehicle, Capital Ship',
        '<strong>Shielded</strong><br><strong>When Played:</strong> Give an Experience token to each friendly unit for each upgrade on it. Then, heal 1 damage from your base for each friendly upgrade.',
        '/images/cards/chimaera-grand-admirals-might.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-030',
        'Lethal Crackdown',
        NULL,
        'Event',
        'Vigilance, Villainy',
        'Uncommon',
        4, NULL, NULL,
        'Tactic',
        'Defeat a non-leader unit. Deal damage to your base equal to that unit''s power.',
        '/images/cards/lethal-crackdown.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-031',
        'Bb-8',
        'Frazzled Friend',
        'Unit',
        'Vigilance, Heroism',
        'Rare',
        1, 0, 4,
        'Resistance, Droid',
        '<strong>On Attack:</strong> Heal 1 damage from a friendly unit. If you do, deal 1 damage to an enemy unit.',
        '/images/cards/bb-8-frazzled-friend.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-032',
        'Kaydel Connix',
        'Keep Going!',
        'Unit',
        'Vigilance, Heroism',
        'Common',
        2, 1, 5,
        'Resistance',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)',
        '/images/cards/kaydel-connix-keep-going.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-033',
        'Clan Wren Rescuer',
        NULL,
        'Unit',
        'Vigilance, Heroism',
        'Common',
        2, 1, 2,
        'Mandalorian, Trooper',
        '<strong>When Played:</strong> Give an Experience token to a unit.',
        '/images/cards/clan-wren-rescuer.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-034',
        'Obi-Wan''s Interceptor',
        'Nothing Too Fancy',
        'Unit',
        'Vigilance, Heroism',
        'Uncommon',
        2, 2, 3,
        'Jedi, Republic, Vehicle, Fighter',
        'Other friendly <strong>Republic</strong> units get +0/+1.',
        '/images/cards/obi-wans-interceptor-nothing-too-fancy.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-035',
        'Yoda',
        'A Disturbing Puzzle',
        'Unit',
        'Vigilance, Heroism',
        'Legendary',
        3, 3, 4,
        'Force, Jedi, Republic',
        '<strong>On Attack:</strong> You may choose an enemy unit. Prevent all combat damage that would be dealt to and dealt by that unit for this phase.',
        '/images/cards/yoda-a-disturbing-puzzle.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-036',
        'Concord Dawn Interceptors',
        NULL,
        'Unit',
        'Vigilance, Heroism',
        'Uncommon',
        3, 1, 4,
        'Mandalorian, Vehicle, Fighter',
        '<strong>Sentinel</strong> (Units in this arena can''t attack your non-Sentinel units or your base.)<br>This unit gets +2/+0 while defending.',
        '/images/cards/concord-dawn-interceptors.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-037',
        'Finn',
        'Stand And Fight',
        'Unit',
        'Vigilance, Heroism',
        'Rare',
        4, 4, 5,
        'Resistance',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)<br>While this unit is upgraded, he gains <strong>Restore 2</strong>. (When he attacks, heal 2 damage from your base.)',
        '/images/cards/finn-stand-and-fight.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-038',
        'Admiral Holdo',
        'We Are The Spark',
        'Unit',
        'Vigilance, Heroism',
        'Rare',
        5, 3, 7,
        'Resistance, Official',
        'Draw 1 more card during the regroup phase.',
        '/images/cards/admiral-holdo-we-are-the-spark.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-039',
        'Ahsoka Tano',
        'Not On My Death Watch',
        'Unit',
        'Vigilance, Heroism',
        'Legendary',
        5, 4, 5,
        'Force, Jedi, Republic',
        '<strong>When Played:</strong> You may defeat a unit that costs 3 or less.',
        '/images/cards/ahsoka-tano-not-on-my-death-watch.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-040',
        'Rose Tico',
        'In The Fight',
        'Unit',
        'Vigilance, Heroism',
        'Uncommon',
        6, 6, 6,
        'Resistance',
        '<strong>Shielded</strong> (When you play this unit, give a Shield token to her.)<strong>On Attack:</strong> You may give a Shield token to another <strong>RESISTANCE</strong> unit.',
        '/images/cards/rose-tico-in-the-fight.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-041',
        'Captain Rex',
        'Staunch Advocate',
        'Unit',
        'Vigilance, Heroism',
        'Rare',
        7, 7, 5,
        'Republic, Clone, Trooper',
        '<strong>Sentinel</strong><br><strong>Shielded</strong><br><strong>Restore 3</strong>',
        '/images/cards/captain-rex-staunch-advocate.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-042',
        'Obi-Wan Kenobi',
        'I Sense A Trap',
        'Unit',
        'Vigilance, Heroism',
        'Legendary',
        8, 5, 9,
        'Force, Jedi, Republic',
        '<strong>When Played:</strong> You may choose one:<br>- Play a unit that was defeated this phase from your discard pile. It costs 5 less./r- Choose a unit that entered play this phase. It gets -5/-5 for this phase.',
        '/images/cards/obi-wan-kenobi-i-sense-a-trap.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-043',
        'Raddus',
        'Hope Of The Resistance',
        'Unit',
        'Vigilance, Heroism',
        'Rare',
        9, 8, 9,
        'Resistance, Vehicle, Capital Ship',
        '<strong>Restore 3</strong> (When this unit attacks, heal 3 damage from your base.)<br>While this unit is exhausted, cards your opponents play cost <strong>3</strong> more.',
        '/images/cards/raddus-hope-of-the-resistance.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-044',
        'Hello There',
        NULL,
        'Event',
        'Vigilance, Heroism',
        'Uncommon',
        3, NULL, NULL,
        'Trick',
        'Choose a unit that entered play this phase. It gets -4/-4 for this phase.',
        '/images/cards/hello-there.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-045',
        'Qi''ra',
        'Survivor',
        'Unit',
        'Vigilance, Vigilance',
        'Rare',
        3, 1, 1,
        'Underworld',
        '<strong>When Played:</strong> Give 2 Experience tokens to this unit.<br><strong>When Defeated:</strong> Choose a unit. You may give an Experience token to it for each Experience token that was on this unit.',
        '/images/cards/qira-survivor.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-046',
        'Vigilant Defense',
        NULL,
        'Event',
        'Vigilance, Vigilance',
        'Uncommon',
        4, NULL, NULL,
        'Tactic',
        'Give a friendly unit <strong>+0/+3</strong> and <strong>SENTINEL</strong> for this phase.<br>Heal 3 damage from a friendly unit or base.<br>Defeat an enemy unit with <strong>3</strong> or less remaining HP.',
        '/images/cards/vigilant-defense.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-047',
        'Moisture Farmer',
        NULL,
        'Unit',
        'Vigilance',
        'Common',
        1, 0, 4,
        'Fringe',
        '<strong>Restore 2</strong> (When this unit attacks, heal 2 damage from your base.)',
        '/images/cards/moisture-farmer.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-048',
        'Desperate Commando',
        NULL,
        'Unit',
        'Vigilance',
        'Common',
        2, 2, 2,
        'Resistance, Trooper',
        '<strong>When Defeated:</strong> You may give a unit -1/-1 for this phase.',
        '/images/cards/desperate-commando.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-049',
        'Dexter Jettster',
        'Obi-Wannnn!!!',
        'Unit',
        'Vigilance',
        'Uncommon',
        3, 2, 6,
        'Fringe',
        '<strong>On Attack:</strong> You may pay 1. If you do, look at the top 3 cards of your deck. Put any number of them on the bottom of your deck and the rest on top in any order.',
        '/images/cards/dexter-jettster-obi-wannnn.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-050',
        'Rotunda Senate Guards',
        NULL,
        'Unit',
        'Vigilance',
        'Common',
        4, 4, 5,
        'Republic, Trooper',
        'While this unit is undamaged, it gains <strong>Sentinel</strong>. (Enemy units in this arena must attack a Sentinel when they attack you.)',
        '/images/cards/rotunda-senate-guards.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-051',
        'Munificent Frigate',
        NULL,
        'Unit',
        'Vigilance',
        'Common',
        5, 4, 7,
        'Separatist, Vehicle, Capital Ship',
        NULL,
        '/images/cards/munificent-frigate.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-052',
        'Resilient',
        NULL,
        'Upgrade',
        'Vigilance',
        'Common',
        1, NULL, NULL,
        'Innate',
        NULL,
        '/images/cards/resilient.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-053',
        'Disciples'' Devotion',
        NULL,
        'Upgrade',
        'Vigilance',
        'Uncommon',
        3, NULL, NULL,
        'Innate',
        'While attached unit is exhausted, it gains <strong>Sentinel</strong>. (Enemy units in this arena must attack a Sentinel when they attack you.)',
        '/images/cards/disciples-devotion.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-054',
        'Moment of Peace',
        NULL,
        'Event',
        'Vigilance',
        'Common',
        1, NULL, NULL,
        'Innate',
        'Give a Shield token to a unit.',
        '/images/cards/moment-of-peace.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-055',
        'Betrayed Trust',
        NULL,
        'Event',
        'Vigilance',
        'Uncommon',
        2, NULL, NULL,
        'Trick',
        'Choose an enemy unit. For this phase, that unit can''t deal combat damage.',
        '/images/cards/betrayed-trust.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-056',
        'Allegiant General Pryde',
        'Heartless Efficiency',
        'Unit',
        'Command, Villainy',
        'Uncommon',
        1, 2, 2,
        'First Order, Official',
        '<strong>When Defeated:</strong> Give an Experience token to the friendly unit with the most power. (If multiple units are tied, choose one.)',
        '/images/cards/allegiant-general-pryde-heartless-efficiency.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-057',
        'Director Krennic',
        'Architect Of Annihilation',
        'Unit',
        'Command, Villainy',
        'Common',
        2, 0, 4,
        'Imperial, Official',
        '<strong>Action Exhaust:</strong> Play a unit that costs 7 or more from your hand. It costs 2 less.',
        '/images/cards/director-krennic-architect-of-annihilation.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-058',
        'General Veers',
        'Prepared',
        'Unit',
        'Command, Villainy',
        'Uncommon',
        2, 3, 3,
        'Imperial, Official',
        '<strong>Overwhelm</strong> (When attacking an enemy unit, deal excess damage to the opponent''s base.)',
        '/images/cards/general-veers-prepared.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-059',
        'Exegol Patroller',
        NULL,
        'Unit',
        'Command, Villainy',
        'Common',
        2, 3, 1,
        'First Order, Sith, Vehicle, Fighter',
        '<strong>Overwhelm</strong> (When attacking an enemy unit, deal excess damage to the opponent''s base.)',
        '/images/cards/exegol-patroller.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-060',
        'Jabba The Hutt',
        'Bounties Left And Right',
        'Unit',
        'Command, Villainy',
        'Rare',
        3, 0, 8,
        'Underworld, Hutt',
        '<strong>Action Exhaust:</strong> Choose an enemy unit. For this phase, while that unit is defending, the attacker gets +4/+0.',
        '/images/cards/jabba-the-hutt-bounties-left-and-right.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-061',
        'Knight of Ren',
        NULL,
        'Unit',
        'Command, Villainy',
        'Common',
        3, 4, 4,
        'First Order',
        NULL,
        '/images/cards/knight-of-ren.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-062',
        'Admiral Motti',
        'Conan In Command',
        'Unit',
        'Command, Villainy',
        'Rare',
        4, 2, 6,
        'Imperial, Official',
        '<strong>Hidden</strong> (This unit can''t be attacked if he was played this phase.)<br><strong>On Attack:</strong> Give an Experience token to each other friendly <strong>Imperial</strong> unit.',
        '/images/cards/admiral-motti-conan-in-command.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-063',
        'Kylo''s TIE Silencer',
        'Follow My Lead',
        'Unit',
        'Command, Villainy',
        'Rare',
        4, 4, 4,
        'First Order, Vehicle, Fighter',
        '<strong>Restore 1</strong> (When this unit attacks, heal 1 damage from your base.)<br><strong>When this unit is discarded from your hand:</strong> For this phase, you may play it from your discard pile (paying its cost).',
        '/images/cards/kylos-tie-silencer-follow-my-lead.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-064',
        'Captain Phasma',
        'On My Command',
        'Unit',
        'Command, Villainy',
        'Uncommon',
        5, 5, 6,
        'First Order, Trooper',
        '<strong>When Played/On Attack:</strong> You may give another <strong>First Order</strong> unit +2/+2 for this phase.',
        '/images/cards/captain-phasma.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-065',
        'Darth Maul',
        'Mindful Apprentice',
        'Unit',
        'Command, Villainy',
        'Rare',
        6, 4, 7,
        'Force, Sith',
        '<strong>Sentinel</strong><br><strong>Overwhelm</strong><br><strong>Raid</strong> 3',
        '/images/cards/darth-maul-mindful-apprentice.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-066',
        'Mother Talzin',
        'The Time Has Come',
        'Unit',
        'Command, Villainy',
        'Rare',
        7, 5, 5,
        'Force, Night',
        '<strong>When Played:</strong> You may play a Villainy unit that costs 4 or less from your discard pile for free. If it''s not a <strong>Night</strong> or <strong>Undead</strong> unit, deal damage equal to its cost to this unit.',
        '/images/cards/mother-talzin-the-time-has-come.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-067',
        'Darth Vader',
        'Useless To Resist',
        'Unit',
        'Command, Villainy',
        'Legendary',
        8, 8, 8,
        'Force, Imperial, Sith',
        '<strong>Ambush</strong><br>Each other friendly unit gains <strong>Ambush</strong>.',
        '/images/cards/darth-vader-useless-to-resist.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-068',
        'Devastator',
        'Lord Vader''s Flagship',
        'Unit',
        'Command, Villainy',
        'Legendary',
        10, 9, 9,
        'Imperial, Vehicle, Capital Ship',
        '<strong>Overwhelm</strong><br><strong>When Played:</strong> Search the top 5 cards of your deck for a Villainy unit that costs 12 or less and play it for free. (Put the other cards on the bottom of your deck in a random order.)',
        '/images/cards/devastator-lord-vaders-flagship.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-069',
        'Apology Accepted',
        NULL,
        'Event',
        'Command, Villainy',
        'Uncommon',
        1, NULL, NULL,
        'Tactic',
        'Defeat a friendly unit. You may give 2 Experience tokens to a unit.',
        '/images/cards/apology-accepted.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-070',
        'C-3Po',
        'Impulse Buy',
        'Unit',
        'Command, Heroism',
        'Rare',
        1, 1, 3,
        'Rebel, Droid',
        '<strong>Action Exhaust:</strong> Give an Experience token to another unit. Then, if it has 6 or more power, draw a card.',
        '/images/cards/c-3po-impulse-buy.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-071',
        'Avar Kriss',
        'For Light And Life',
        'Unit',
        'Command, Heroism',
        'Rare',
        2, 0, 5,
        'Force, Jedi, Republic',
        '<strong>Raid 1</strong> (This unit gets +1/+0 while attacking.)<br>This unit gains <strong>Raid 1</strong> for each other friendly unit.',
        '/images/cards/avar-kriss-for-light-and-life.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-072',
        'Lor San Tekka',
        'Secret Keeper',
        'Unit',
        'Command, Heroism',
        'Uncommon',
        2, 3, 2,
        'Resistance',
        '<strong>When Defeated:</strong> You may give an Experience token to a Unique unit.',
        '/images/cards/lor-san-tekka.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-073',
        'Phoenix Squadron A-Wing',
        NULL,
        'Unit',
        'Command, Heroism',
        'Common',
        2, 3, 2,
        'Rebel, Vehicle, Fighter',
        NULL,
        '/images/cards/phoenix-squadron-a-wing.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-074',
        'Advanced Recon Commando',
        NULL,
        'Unit',
        'Command, Heroism',
        'Common',
        3, 4, 3,
        'Republic, Clone, Trooper',
        '<strong>Overwhelm</strong> (When attacking an enemy unit, deal excess damage to the opponent''s base.)',
        '/images/cards/advanced-recon-commando.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-075',
        'Red Leader',
        'Wait For My Signal',
        'Unit',
        'Command, Heroism',
        'Uncommon',
        3, 2, 4,
        'Rebel, Vehicle, Fighter',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)<br><strong>Restore 2</strong> (When this unit attacks, heal 2 damage from your base.)',
        '/images/cards/red-leader-wait-for-my-signal.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-076',
        'Clone Commander Cody',
        'Loyal To The Republic',
        'Unit',
        'Command, Heroism',
        'Rare',
        4, 4, 4,
        'Republic, Clone, Trooper',
        '\When another friendly Republic unit enters play:| Give an Experience token to it.',
        '/images/cards/clone-commander-cody-loyal-to-the-republic.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-077',
        'Mon Mothma',
        'Truth Is An Abyss',
        'Unit',
        'Command, Heroism',
        'Legendary',
        4, 3, 6,
        'Rebel, Official',
        '<strong>When Played:</strong> You may attack with another unit, even if it''s exhausted. For this attack, it gains <strong>Overwhelm</strong> and can''t attack bases.',
        '/images/cards/mon-mothma-truth-is-an-abyss.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-078',
        'Anakin Skywalker',
        'Destined For Darkness',
        'Unit',
        'Command, Heroism',
        'Legendary',
        5, 7, 4,
        'Force, Jedi, Republic',
        '<strong>When Defeated:</strong> Search your deck for a card named Darth Vader, reveal it, and draw it.<br>While this unit is in your discard pile, ignore the aspect penalties on cards you play named Darth Vader.',
        '/images/cards/anakin-skywalker-destined-for-darkness.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-079',
        'Qui-Gon Jinn',
        'Unwavering Belief',
        'Unit',
        'Command, Heroism',
        'Uncommon',
        5, 5, 5,
        'Force, Jedi, Republic',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)<br><strong>When Played:</strong> Give another friendly unit +2/+2 for this phase.',
        '/images/cards/qui-gon-jinn-unwavering-belief.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-080',
        'Bright Hope',
        'Narrow Escape',
        'Unit',
        'Command, Heroism',
        'Starter',
        6, 4, 6,
        'Rebel, Vehicle, Transport',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)',
        '/images/cards/bright-hope.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-081',
        'Mace Windu',
        'Dropping By',
        'Unit',
        'Command, Heroism',
        'Legendary',
        6, 5, 5,
        'Force, Jedi, Republic',
        '<strong>Ambush</strong><br><strong>On Attack:</strong> Give an Experience token to this unit.',
        '/images/cards/mace-windu-dropping-by.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-082',
        'Luke Skywalker',
        'Do You Read Me?',
        'Unit',
        'Command, Heroism',
        'Starter',
        7, 6, 6,
        'Force, Rebel',
        '<strong>Restore 2</strong> (When this unit attacks, heal 2 damage from your base.)<br><strong>When Played:</strong> You may deal 3 damage to a ground unit.',
        '/images/cards/luke-skywalker.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-083',
        'Padmé Amidala',
        'With Great Pleasure',
        'Unit',
        'Command, Heroism',
        'Rare',
        8, 5, 7,
        'Naboo, Republic, Official',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)<br>Other friendly <strong>Republic</strong> units get +3/+3.',
        '/images/cards/padme-amidala-with-great-pleasure.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-084',
        'Negotiator',
        'Prepare The Troops',
        'Unit',
        'Command, Heroism',
        'Rare',
        9, 9, 7,
        'Republic, Vehicle, Capital Ship',
        '<strong>Sentinel</strong> (Enemy units in this arena must attack a Sentinel when they attack you.)<br><strong>On Attack/When Defeated:</strong> Resource the top card of your deck. Then, you may return a friendly resource to its owner''s hand.',
        '/images/cards/negotiator-prepare-the-troops.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-085',
        'Inspiring Mentor',
        NULL,
        'Upgrade',
        'Command, Heroism',
        'Rare',
        2, NULL, NULL,
        'Innate',
        'Attach to a non-<strong>Vehicle</strong> unit.<br>Attached unit gains: "<strong>On Attack/When Defeated:</strong> Give an Experience token to another friendly unit."',
        '/images/cards/inspiring-mentor.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-086',
        'Enfys Nest',
        'No More!',
        'Unit',
        'Command, Command',
        'Rare',
        5, 4, 6,
        'Underworld',
        '<strong>Ambush</strong> (When you play this unit, she may attack an enemy unit.)<br>While attacking a unit that costs 3 or less, this unit gets +2/+0 and gains <strong>Overwhelm</strong>.',
        '/images/cards/enfys-nest-no-more.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-087',
        'Issue A Command',
        NULL,
        'Event',
        'Command, Command',
        'Uncommon',
        4, NULL, NULL,
        'Plan',
        'Reveal the top card of your deck. If it''s a unit, draw it.<br>Give an Experience token to a friendly unit.<br>Resource this event.',
        '/images/cards/issue-a-command.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-088',
        'Senator''s Aide',
        NULL,
        'Unit',
        'Command',
        'Common',
        1, 0, 3,
        'Republic, Official',
        'While you have the initiative, this unit gets +2/+0.',
        '/images/cards/senators-aide.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-089',
        'Maz Kanata',
        'It Calls To You',
        'Unit',
        'Command',
        'Uncommon',
        2, 2, 1,
        'Underworld',
        '<strong>Hidden</strong> (This unit can''t be attacked if she was played this phase.)<br><strong>When you resource a card</strong> (including during the regroup phase): Give an Experience token to this unit.',
        '/images/cards/maz-kanata-it-calls-to-you.webp',
        NULL
    ),

    (
        (SELECT id FROM sets WHERE code = 'IC27'),
        'IC27-090',
        'Homestead Militia',
        NULL,
        'Unit',
        'Command',
        'Common',
        3, 3, 4,
        'Fringe, Trooper',
        'While you control 6 or more resources, this unit gains <strong>Sentinel</strong>. (Units in this arena can''t attack your non-Sentinel units or your base.)',
        '/images/cards/homestead-militia.webp',
        NULL
    )
ON CONFLICT (card_number) DO NOTHING;

