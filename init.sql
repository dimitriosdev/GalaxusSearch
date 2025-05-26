-- init.sql: Example seed for PostgreSQL

CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    price NUMERIC NOT NULL,
    description TEXT NOT NULL,
    category TEXT NOT NULL,
    brand TEXT NOT NULL,
    sku TEXT NOT NULL,
    stock INTEGER NOT NULL,
    created_at TIMESTAMP NOT NULL
);

-- Example seed data (replace or expand as needed)
INSERT INTO products (name, price, description, category, brand, sku, stock, created_at) VALUES
('Product 1', 9.99, 'Sample description for Product 1 in Electronics by Acme.', 'Electronics', 'Acme', 'SKU-000001', 100, NOW()),
('Product 2', 19.99, 'Sample description for Product 2 in Home Appliances by Globex.', 'Home Appliances', 'Globex', 'SKU-000002', 200, NOW()),
('Product 3', 29.99, 'Sample description for Product 3 in Furniture by Umbrella.', 'Furniture', 'Umbrella', 'SKU-000003', 300, NOW())
ON CONFLICT DO NOTHING;

-- Insert 10,000 products with unique names and generated values
do $$
declare
  categories text[] := array['Electronics', 'Home Appliances', 'Furniture', 'Sportswear', 'Toys', 'Books', 'Garden', 'Automotive'];
  brands text[] := array['Acme', 'Globex', 'Umbrella', 'Initech', 'Soylent', 'Stark', 'Wayne', 'Wonka'];
  i int;
  cat text;
  brand text;
  price numeric;
  stock int;
  created_at timestamp;
  sku text;
begin
  for i in 4..10003 loop
    cat := categories[1 + (random() * 7)::int];
    brand := brands[1 + (random() * 7)::int];
    price := round((random() * 2000 + 5)::numeric, 2);
    stock := (random() * 1000)::int;
    created_at := NOW() - ((random() * 365)::int || ' days')::interval;
    sku := 'SKU-' || lpad(i::text, 6, '0');
    insert into products (name, price, description, category, brand, sku, stock, created_at)
    values (
      'Product ' || i,
      price,
      'Sample description for Product ' || i || ' in ' || cat || ' by ' || brand || '.',
      cat,
      brand,
      sku,
      stock,
      created_at
    );
  end loop;
end;
$$;
