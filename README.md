# WikiLive Backend

## Запуск через Visual Studio + Docker
1. Откройте `WikiLive.sln` в Visual Studio 2022.
2. Убедитесь, что установлен workload **ASP.NET and web development** и компонент **Container Development Tools**.
3. В качестве startup project выберите **docker-compose**.
4. Нажмите `F5` или `Ctrl+F5`.
5. Swagger откроется по адресу `http://localhost:5099/swagger`.

## Запуск через терминал
```bash
docker compose up --build
```

## Что нужно настроить для реального MWS API
В файле `docker-compose.yml` замените:
- `Mws__BaseUrl`
- `Mws__Token`

## Полезные адреса
- API / Swagger: `http://localhost:5099/swagger`
- PostgreSQL: `localhost:5432`

## Примечание
`docker-compose.yml` лежит в корне решения, поэтому Visual Studio видит compose-проект корректно.
