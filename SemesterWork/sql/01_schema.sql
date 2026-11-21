-- Схема базы данных Star Wars: Unlimited DB (PostgreSQL)

CREATE TABLE IF NOT EXISTS sets (
    id SERIAL PRIMARY KEY,
    code VARCHAR(10) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    release_date DATE NOT NULL,
    description TEXT
);

CREATE TABLE IF NOT EXISTS cards (
    id SERIAL PRIMARY KEY,
    set_id INT NOT NULL REFERENCES sets(id) ON DELETE CASCADE,
    card_number VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(120) NOT NULL,
    subtitle VARCHAR(120),
    card_type VARCHAR(40) NOT NULL,
    aspect VARCHAR(60) NOT NULL,
    rarity VARCHAR(30) NOT NULL,
    cost INT NOT NULL CHECK (cost >= 0 AND cost <= 20),
    power INT CHECK (power >= 0 AND power <= 30),
    hp INT CHECK (hp >= 0 AND hp <= 50),
    traits VARCHAR(200),
    ability_text TEXT,
    image_url VARCHAR(300),
    artist VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(120) UNIQUE NOT NULL,
    password_hash VARCHAR(128) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'User' CHECK (role IN ('User', 'Admin')),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS comments (
    id SERIAL PRIMARY KEY,
    card_id INT NOT NULL REFERENCES cards(id) ON DELETE CASCADE,
    user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(120) NOT NULL,
    body TEXT NOT NULL,
    rating INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_cards_set_id ON cards(set_id);
CREATE INDEX IF NOT EXISTS idx_cards_aspect ON cards(aspect);
CREATE INDEX IF NOT EXISTS idx_cards_card_type ON cards(card_type);
CREATE INDEX IF NOT EXISTS idx_comments_card_id ON comments(card_id);
CREATE INDEX IF NOT EXISTS idx_comments_user_id ON comments(user_id);
