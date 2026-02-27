
--ISBN indexes
CREATE INDEX IF NOT EXISTS idx_publications_isbn ON lnc_publications (isbn) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_covers_isbn ON lnc_covers (isbn) WHERE deleted_at IS NULL;

--Create FK indexes
CREATE INDEX IF NOT EXISTS idx_publications_volume_id ON lnc_publications (volume_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_publications_publisher_id ON lnc_publications (publisher_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_volumes_series_id ON lnc_volumes (series_id) WHERE deleted_at IS NULL;

--Create created_at indexes
CREATE INDEX IF NOT EXISTS idx_publications_created_at ON lnc_publications (created_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_covers_created_at ON lnc_covers (created_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_publishers_created_at ON lnc_publishers (created_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_series_created_at ON lnc_series (created_at) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_volumes_created_at ON lnc_volumes (created_at) WHERE deleted_at IS NULL;
