-- DDL para Travelio MVP (PostgreSQL)

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE providers (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  code TEXT NOT NULL UNIQUE,
  name TEXT NOT NULL,
  base_url TEXT,
  metadata JSONB,
  created_at TIMESTAMPTZ DEFAULT now()
);

CREATE TABLE raw_provider_responses (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  provider_id UUID REFERENCES providers(id) ON DELETE SET NULL,
  request_payload JSONB,
  response_payload JSONB,
  status_code INT,
  created_at TIMESTAMPTZ DEFAULT now()
);

CREATE TABLE offers_normalized (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  provider_id UUID REFERENCES providers(id) ON DELETE SET NULL,
  provider_code TEXT NOT NULL,
  provider_offer_id TEXT,
  service_type TEXT,
  price_amount NUMERIC(12,2),
  currency TEXT,
  availability_count INT,
  start_date DATE,
  end_date DATE,
  raw_json JSONB,
  search_hash TEXT,
  created_at TIMESTAMPTZ DEFAULT now(),
  updated_at TIMESTAMPTZ DEFAULT now()
);
CREATE INDEX idx_offers_search_hash ON offers_normalized(search_hash);

CREATE TABLE searches (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  client_id TEXT,
  search_criteria JSONB,
  created_at TIMESTAMPTZ DEFAULT now()
);

CREATE TABLE pre_bookings (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  client_id TEXT,
  search_id UUID REFERENCES searches(id) ON DELETE SET NULL,
  offer_id UUID REFERENCES offers_normalized(id) ON DELETE SET NULL,
  provider_prebooking_id TEXT,
  expires_at TIMESTAMPTZ,
  status TEXT,
  lock_token TEXT,
  created_at TIMESTAMPTZ DEFAULT now(),
  updated_at TIMESTAMPTZ DEFAULT now()
);
CREATE INDEX idx_prebookings_expires_at ON pre_bookings(expires_at);

CREATE TABLE bookings (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  client_id TEXT,
  pre_booking_id UUID REFERENCES pre_bookings(id) ON DELETE SET NULL,
  provider_booking_id TEXT UNIQUE,
  idempotency_key TEXT,
  status TEXT,
  amount NUMERIC(12,2),
  currency TEXT,
  metadata JSONB,
  created_at TIMESTAMPTZ DEFAULT now(),
  updated_at TIMESTAMPTZ DEFAULT now()
);
CREATE UNIQUE INDEX uq_bookings_client_idempotency_key ON bookings(client_id, idempotency_key) WHERE idempotency_key IS NOT NULL;
CREATE UNIQUE INDEX uq_bookings_pre_booking_id ON bookings(pre_booking_id) WHERE pre_booking_id IS NOT NULL;

CREATE TABLE idempotency_keys (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  key TEXT NOT NULL,
  client_id TEXT,
  request_hash TEXT NOT NULL,
  result_booking_id UUID REFERENCES bookings(id) ON DELETE SET NULL,
  created_at TIMESTAMPTZ DEFAULT now(),
  expires_at TIMESTAMPTZ
);
CREATE UNIQUE INDEX uq_idempotency_keys_client_key ON idempotency_keys(client_id, key);

CREATE TABLE audit_events (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  aggregate_type TEXT,
  aggregate_id UUID,
  event_type TEXT,
  payload JSONB,
  created_at TIMESTAMPTZ DEFAULT now()
);

-- Índices adicionales recomendados
CREATE INDEX idx_bookings_status ON bookings(status);
CREATE INDEX idx_offers_dates_price ON offers_normalized(start_date, end_date, price_amount);
