# Centos
rpm -ivh http://yum.postgresql.org/9.4/redhat/rhel-7-x86_64/pgdg-centos94-9.4-1.noarch.rpm
yum install -y postgresql94 postgresql94-server postgresql94-libs postgresql94-contrib postgresql94-devel
rpm -ivh http://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm
sudo yum install -y postgis2_94
/usr/pgsql-9.4/bin/postgresql94-setup initdb
systemctl start postgresql-9.4.service
systemctl enable postgresql-9.4.service
firewall-cmd --permanent --zone=public --add-service=postgresql
systemctl restart firewalld.service
# edit /var/lib/pgsql/9.4/data/postgresql.conf listen_addresses = '*' port = 5432
# edit /var/lib/pgsql/9.4/data/pg_hba.conf host all all         0.0.0.0/0 md5

# Linux Mint 17.2/Ubuntu 14.04
echo deb http://apt.postgresql.org/pub/repos/apt/ trusty-pgdg main>/etc/apt/sources.list.d/pgdg.list
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | \
  sudo apt-key add -
sudo apt-get update
sudo apt-get install postgresql-9.4 postgresql-9.4-postgis-2.1 postgresql-contrib
# edit /etc/postgresql/9.4/main/postgresql.conf listen_addresses = '*' port = 5432
# edit /etc/postgresql/9.4/main/pg_hba.conf host all all         0.0.0.0/0 md5


# Common


#su postgres
sudo -u postgres psql postgres
\password postgres
\q

sudo -u postgres createdb spurious
sudo -u postgres psql spurious

CREATE ROLE testuser WITH SUPERUSER LOGIN PASSWORD 'test';
create extension adminpack;
create extension postgis;
create table subdivisions (
  id integer not null,
  population integer NOT NULL DEFAULT 0,
  boundry geography,
  boundary_gml text,
  beer_volume bigint NOT NULL DEFAULT 0,
  wine_volume bigint NOT NULL DEFAULT 0,
  spirits_volume bigint NOT NULL DEFAULT 0,
  province text,
  name text,
  average_income integer NOT NULL DEFAULT 0,
  median_income integer NOT NULL DEFAULT 0,
  median_after_tax_income integer NOT NULL DEFAULT 0,
  average_after_tax_income integer NOT NULL DEFAULT 0,
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

# Example queries

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

-- centre of subdiv
select st_asgeojson(st_centroid(boundry::geometry)) as centre from subdivisions
where province = 'Ontario';