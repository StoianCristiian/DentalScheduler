# AI-Driven Appointment Scheduler 

Un sistem modern de programări pentru cabinete stomatologice, bazat pe **.NET 10**, **Blazor WebAssembly** și **Inteligență Artificială (Python)**. Proiectul folosește o arhitectură modulară și scalabilă, respectând principiile **Clean Architecture** și **CQRS**.

---

## Funcționalități Principale

### Modul AI (Python)
- **Sugestii Inteligente:** Propune automat intervalul optim pentru pacient bazat pe istoric și preferințe.
- **Optimizare Orar:** Reduce suprapunerile și optimizează agenda medicilor.

### Backend (.NET API)
- **Gestiune Programări:** CRUD complet folosind CQRS.
- **Personal & Servicii:** Administrarea medicilor și a tipurilor de intervenții.
- **Monetizare:** Suport pentru taxe de rezervare sau abonamente. **(INTEGRAT CU STRIPE)**
- **Plăți Online:** Integrare completă cu **Stripe** pentru plăți securizate cu cardul, inclusiv Webhooks pentru confirmare automată.

### Frontend (Blazor WASM)
- **Interfață Modernă:** Design responsiv și interactiv.
- **Dashboard:** Statistici și grafice pentru administrarea cabinetului.
- **Portal Pacient:** Programări online, istoric medical și plăți integrate.

---

##  Arhitectură & Tehnologii

Proiectul este structurat pe **Microservicii** și **Clean Architecture**:

### 1. Backend (.NET 10)
- **DentalScheduler.Domain:** Entități și logică de business pură (Core).
- **DentalScheduler.Application:** Cazuri de utilizare, **CQRS** (cu **MediatR**), Validări.
- **DentalScheduler.Infrastructure:** Acces la date (**EF Core**, **SQL Server**), servicii externe.
- **DentalScheduler.Api:** REST API, Controllere, Swagger/Scalar UI.

### 2. Frontend
- **DentalScheduler.Client:** Aplicație **Blazor WebAssembly** (Standalone).

### 3. AI Service
- **DentalScheduler.AI:** Serviciu Python (**FastAPI/Django**) containerizat separat.

### 4. DevOps & Cloud
- **Docker & Docker Compose:** Containerizarea tuturor serviciilor (API, AI, Db, Frontend).
- **CI/CD:** Pipeline pentru build, testare și scanare cu **SonarQube**.
- **Cloud Deployment:** Pregătit pentru Azure/AWS (Azure Container Apps, SQL Database).

---

##  Cerințe și Setup Local

### Prerechizite
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Python 3.10+](https://www.python.org/downloads/)
- Un IDE (JetBrains Rider, Visual Studio 2022, VS Code)

### Cum să rulezi proiectul

1. **Clonează repository-ul**
   ```bash
   git clone <repo-url>
   cd DentalScheduler
   ```

2. **Configurează Variabilele de Mediu**
   Creează un fișier `.env` în rădăcina proiectului și adaugă configurația (inclusiv cheile Stripe):
   ```env
   # Database
   DB_PASSWORD=
   DB_USER=
   ConnectionStrings__DefaultConnection=

   # Stripe Keys (Obține-le din dashboard.stripe.com)
   STRIPE_PUBLISHABLEKEY=pk_test_...
   STRIPE_SECRETKEY=sk_test_...
   STRIPE_WEBHOOK_SECRET=whsec_...
   ```

3. **Pornește Infrastructura (Baza de Date)**
   Asigură-te că Docker Desktop rulează, apoi:
   ```bash
   docker-compose up -d
   ```
   Aceasta va porni SQL Server.

4. **Configurează Backend-ul**
   - Asigură-te că ai fișierul `.env` în rădăcină (vezi `.env.example`).
   - Rulează migrările (automat la pornirea API-ului sau manual):
     ```bash
     cd DentalScheduler.Api
     dotnet run
     ```

4. **Accesează Aplicația**
   - **API Docs (Scalar):** `https://localhost:7174/scalar/v1`
   - **Frontend:** `https://localhost:5289`

---

##  Standarde de Calitate
- **Clean Architecture:** Separarea clară a responsabilităților.
- **CQRS:** Segregarea comenzilor (scrieri) de interogări (citiri).
- **SOLID Principles:** Cod modular și testabil.
- **Testing:** Unit Tests (xUnit/NUnit pentru .NET, pytest pentru Python).

---
*Proiect realizat pentru materia .NET - Facultate, Anul 3, Semestrul 2.*
