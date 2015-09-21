rpm -ivh http://yum.postgresql.org/9.4/redhat/rhel-7-x86_64/pgdg-centos94-9.4-1.noarch.rpm
yum install postgresql94 postgresql94-server postgresql94-libs postgresql94-contrib postgresql94-devel
rpm -ivh http://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm
sudo yum install postgis2_94
/usr/pgsql-9.4/bin/postgresql94-setup initdb

# edit /var/lib/pgsql/9.4/data/postgresql.conf listen_addresses = '*' port = 5432
# edit /var/lib/pgsql/9.4/data/pg_hba.conf host all all         0.0.0.0/0 md5

systemctl start postgresql-9.4.service
systemctl enable postgresql-9.4.service
firewall-cmd --permanent --zone=public --add-service=postgresql
systemctl restart firewalld.service
su postgres

createdb spurious
psql spurious

CREATE ROLE testuser WITH SUPERUSER LOGIN PASSWORD 'test';
create extension adminpack;
create extension postgis;
create table subdivisions (
  id integer not null,
  population integer,
  boundry geography,
  boundary_gml text,
  beer_volume bigint,
  wine_volume bigint,
  spirits_volume bigint,
  province text,
  name text,
  CONSTRAINT firstkey PRIMARY KEY (id)
);

CREATE INDEX idx_boundary
  ON subdivisions
  USING gist
  (boundry);

-- Table: stores

-- DROP TABLE stores;

CREATE TABLE stores
(
  id integer NOT NULL,
  name text,
  city text,
  beer_volume integer,
  wine_volume integer,
  spirits_volume integer,
  location geography,
  latitude numeric(7,4),
  longitude numeric(7,4),
  CONSTRAINT stores_pkey PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE stores
  OWNER TO jmclachl;

  CREATE INDEX idx_location
  ON stores
  USING gist
  (location);

CREATE TABLE products
(
  id integer NOT NULL,
  name text,
  category text,
  volume integer,
  PRIMARY KEY (id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE products
  OWNER TO jmclachl;
  
CREATE TABLE inventories
(
  product_id integer NOT NULL,
  store_id integer NOT NULL,
  quantity integer,
  CONSTRAINT inventories_pkey PRIMARY KEY (product_id, store_id),
  CONSTRAINT inventories_product_id_fkey FOREIGN KEY (product_id)
      REFERENCES products (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE CASCADE,
  CONSTRAINT inventories_store_id_fkey FOREIGN KEY (store_id)
      REFERENCES stores (id) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE CASCADE
)
WITH (
  OIDS=FALSE
);
ALTER TABLE inventories
  OWNER TO jmclachl;

-- Index: fki_inventories_stores_id_fkey

-- DROP INDEX fki_inventories_stores_id_fkey;

CREATE INDEX fki_inventories_stores_id_fkey
  ON inventories
  USING btree
  (store_id);
  
  
  
  select s.id, s.name, city, 
--p.name, 
p.category, sum(p.volume) as total_volume from stores as s
inner join inventories as i on i.store_id = s.id
inner join products as p on p.id = i.product_id
--where s.id = 243
group by p.category, s.id, s.name, city
order by total_volume;

DROP FUNCTION category_volume(integer,text);

create or replace function category_volume(store_id int, category text) returns int as
$$
declare
	vol int;
begin
	select sum(i.quantity * p.volume) into vol
	from stores s
	inner join inventories i on (i.store_id = s.id)
	inner join products p on (i.product_id = p.id)
	where s.id = $1 and p.category = $2;
	return vol;

end;
$$ language plpgsql;

select s.id, s.name, category_volume(s.id, 'Beer') as beer, category_volume(s.id, 'Wine') as wine, category_volume(s.id, 'Spirit') as spirit
--(select sum(i.quantity * p.volume) where p.category = 'Beer') as beer, p.category
from stores as s
--inner join inventories i on (i.store_id = s.id)
--inner join products p on (i.product_id = p.id)
--where s.id = 243
--and p.category in ('Beer', 'Wine', 'Spirit')
--group by s.id, p.category
;

select s.id, s.name, s.city,
(select sum(i.quantity * p.volume)
	from inventories i
	inner join products p on p.id = i.product_id
	where i.store_id = s.id
	and p.category = 'Beer') as beer_volume,
(select sum(i.quantity * p.volume)
	from inventories i
	inner join products p on p.id = i.product_id
	where i.store_id = s.id and p.category = 'Wine') as wine_volume,
(select sum(i.quantity * p.volume)
	from inventories i
	inner join products p on p.id = i.product_id
	where i.store_id = s.id and p.category = 'Spirit') as spirits_volume
from stores s
order by s.id

update stores set (beer_volume, wine_volume, spirits_volume) = 
(
	(select sum(i.quantity * p.volume)
	from inventories i
	inner join products p on p.id = i.product_id
	where i.store_id = stores.id
	and p.category = 'Beer')

	,(select sum(i.quantity * p.volume)
	from inventories i
	inner join products p on p.id = i.product_id
	where i.store_id = stores.id
	and p.category = 'Wine')

	,(select sum(i.quantity * p.volume)
	from inventories i
	inner join products p on p.id = i.product_id
	where i.store_id = stores.id
	and p.category = 'Spirit')
);

select sb.id, s.id, s.name, s.city
from subdivisions sb
inner join stores s on ST_Intersects(sb.boundry, s.location)
order by sb.id

-- subdivs and volumes
select sb.id, sum(s.beer_volume), sum(s.wine_volume), sum(s.spirits_volume)
from subdivisions sb
inner join stores s on ST_Intersects(sb.boundry, s.location)
group by sb.id
