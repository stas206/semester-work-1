DROP TABLE IF EXISTS cards;
DROP TABLE IF EXISTS monsters;
DROP TABLE IF EXISTS items;
DROP TABLE IF EXISTS rules;
DROP TABLE IF EXISTS strategies;
DROP TABLE IF EXISTS users;

CREATE TABLE monsters (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ImagePath" TEXT NOT NULL
);

CREATE TABLE cards (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ImagePath" TEXT NOT NULL
);

CREATE TABLE items (
    "Id" SERIAL PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "ImagePath" TEXT NOT NULL,
    "Quantity" INTEGER NOT NULL
);

CREATE TABLE rules (
    "Id" SERIAL PRIMARY KEY,
    "Slug" TEXT NOT NULL UNIQUE,
    "Title" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "SortOrder" INTEGER NOT NULL
);

CREATE TABLE strategies (
    "Id" SERIAL PRIMARY KEY,
    "Slug" TEXT NOT NULL UNIQUE,
    "Title" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "Category" TEXT NOT NULL
);

CREATE TABLE users (
    "Id" SERIAL PRIMARY KEY,
    "Username" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Salt" TEXT NOT NULL,
    "Role" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL
);

CREATE INDEX idx_monsters_name ON monsters ("Name");
CREATE INDEX idx_cards_type ON cards ("Type");
CREATE INDEX idx_rules_slug ON rules ("Slug");
CREATE INDEX idx_strategies_slug ON strategies ("Slug");
CREATE INDEX idx_users_username ON users ("Username");
