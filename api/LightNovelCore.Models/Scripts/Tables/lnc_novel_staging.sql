CREATE TABLE IF NOT EXISTS lnc_novel_staging (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	series TEXT NULL,
	series_slug TEXT NULL,
	publisher TEXT NULL,
	publisher_slug TEXT NULL,
	url TEXT NULL,
	title TEXT NULL,
	volume TEXT NULL,
	format INTEGER NOT NULL,
	isbn TEXT NULL,
	release_date TIMESTAMP NOT NULL,
	created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_at TIMESTAMP NULL
);
