-- Preparation for the CDC - Step 1
-- ALTER SYSTEM SET wal_level = logical;

DROP TABLE IF EXISTS customers;
DROP TABLE IF EXISTS orders;

CREATE TABLE customers
(
    customer_id      UUID                    NOT NULL PRIMARY KEY,
    ssn              VARCHAR(11)             NOT NULL,
    email            VARCHAR(255)            NOT NULL,
    user_name        VARCHAR(255)            NOT NULL,
    full_name        VARCHAR(255)            NOT NULL,
    delivery_address VARCHAR(255)            NOT NULL,
    delivery_zipcode VARCHAR(255)            NOT NULL,
    delivery_city    VARCHAR(255)            NOT NULL,
    billing_address  VARCHAR(255)            NULL,
    billing_zipcode  VARCHAR(255)            NULL,
    billing_city     VARCHAR(255)            NULL,
    ts               TIMESTAMP DEFAULT NOW() NOT NULL
);


CREATE TABLE orders
(
    order_id    UUID                    NOT NULL PRIMARY KEY,
    customer_id UUID                    NOT NULL,
    quantity    INT                     NOT NULL,
    unit_price  DECIMAL(9, 2)           NOT NULL,
    ts          TIMESTAMP DEFAULT NOW() NOT NULL
);

CREATE
    OR REPLACE FUNCTION update_ts_column_on_update()
    RETURNS TRIGGER AS
$$
BEGIN
    NEW.ts
        = now();
    RETURN NEW;
END;
$$
    language 'plpgsql';

CREATE TRIGGER customers_timestamp
    BEFORE UPDATE
    ON customers
    FOR EACH ROW
EXECUTE PROCEDURE update_ts_column_on_update();

CREATE TRIGGER orders_timestamp
    BEFORE UPDATE
    ON orders
    FOR EACH ROW
EXECUTE PROCEDURE update_ts_column_on_update();



INSERT INTO customers (customer_id, ssn, email, user_name, full_name, delivery_address,
                       delivery_zipcode, delivery_city,
                       billing_address, billing_zipcode, billing_city)
VALUES ('f56c8938-6452-478b-84b5-803b9ea5152d', '1234 230185', 'martin.schwarz@fake.com',
        'm.schwarz', 'martin Schwarz',
        'Marxergasse 17', '1030', 'Wien', NULL,
        NULL, Null);

INSERT INTO customers (customer_id, ssn, email, user_name, full_name, delivery_address,
                       delivery_zipcode, delivery_city,
                       billing_address, billing_zipcode, billing_city)
VALUES ('31e7a241-d570-4961-981d-4aea2b20d22e', '8755 051290', 'shahab.ganji@fake.com', 'shahab',
        'Shahab Ganji',
        'Marxergasse 17',
        '1030', 'Wien', NULL,
        NULL, Null);


INSERT INTO orders (order_id, customer_id, quantity, unit_price)
VALUES ('941f88b5-933a-4ec9-b691-0cced7807d77', 'f56c8938-6452-478b-84b5-803b9ea5152d', 1, 136.99);

INSERT INTO orders (order_id, customer_id, quantity, unit_price)
VALUES ('39a7daf2-2e4f-4c29-a9e1-853225508b68', '31e7a241-d570-4961-981d-4aea2b20d22e', 1, 136.99);

-- Preparation for the CDC - Step 2
ALTER TABLE public.customers
    REPLICA IDENTITY FULL;
ALTER TABLE public.orders
    REPLICA IDENTITY FULL;
