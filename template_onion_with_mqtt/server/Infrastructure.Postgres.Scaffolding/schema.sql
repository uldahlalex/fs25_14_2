drop schema if exists weatherstation;
create schema if not exists weatherstation;

create table devicelog (
                           deviceid text not null primary key,
                           value numeric not null,
                           unit text not null,
                           timestamp timestamp with time zone not null
)


CREATE TABLE chat."user" (
                             id text NOT NULL,
                             email text NOT NULL,
                             hash text NOT NULL,
                             salt text NOT NULL,
                             role text NOT NULL,
                             CONSTRAINT user_pkey PRIMARY KEY (id)
);
