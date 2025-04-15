# MICROSERVICES WGUTIERREZ

- **KAFKA:**
  - **KAFDROP:** [http://localhost:9000/topic/](http://localhost:9000/)

- **MONGO DB MONITORING:**
  - **MONGO VIEW:**
    - **URL:**
      - [http://localhost:8082/db/Pagos/Pagos](http://localhost:8082/db/Pagos/Pagos)
      - [http://localhost:8081/db/Consultas/Consultas](http://localhost:8081/db/Consultas/Consultas)
    - **USERNAME:** admin  
    - **PASSWORD:** pass

- **TRACING:**
  - **JAEGER UI:** [http://localhost:16686/](http://localhost:16686/)

- **LOGS:**
  - **ELASTICSEARCH:** [http://localhost:9200/](http://localhost:9200/)
  - **KIBANA:** [http://localhost:5601/app/home#/](http://localhost:5601/app/home#/)

- **IDENTITY SERVER:**
  - **KEYCLOAK:**
    - **URL:** [http://localhost:8075/](http://localhost:8075/)
    - **USERNAME:** admin  
    - **PASSWORD:** admin
    - **GENERATE JWT:** curl -X POST "http://localhost:8075/realms/wgutierrez_realm/protocol/openid-connect/token" -H "Content-Type: application/x-www-form-urlencoded" -d "client_id=api-order" -d "grant_type=password" -d "username=wgutierrez" -d "password=password"

- **CONTAINER LOGS:**
  - **DOZZLE:** [http://localhost:9999/](http://localhost:9999/)
