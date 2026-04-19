# GymApp 🏋️

Εφαρμογή διαχείρισης γυμναστηρίου χτισμένη με ASP.NET Core Razor Pages.

## Τεχνολογίες
- ASP.NET Core 10 Razor Pages
- Entity Framework Core 10
- SQL Server Express
- Bootstrap 5
- ClosedXML (Excel export)

## Λειτουργίες

### 👤 Διαχείριση Μελών
- CRUD μελών με soft delete
- Αναζήτηση με όνομα, επώνυμο ή τηλέφωνο
- Φιλτράρισμα ενεργών/ανενεργών μελών
- Ιστορικό πληρωμών ανά μέλος

### 🏋️ Προγράμματα & Εκπαιδευτές
- CRUD προγραμμάτων και εκπαιδευτών
- Πακέτα συνδρομών (4/8/12 συνεδρίες/μήνα)
- Εβδομαδιαίο πρόγραμμα με διαθέσιμες θέσεις

### 📋 Συνδρομές
- Εγγραφή μέλους σε πρόγραμμα
- Επιλογή τμήματος (Πρωί/Απόγευμα)
- Έλεγχος διαθέσιμων θέσεων
- Ανανέωση συνδρομής
- Αυτόματη απενεργοποίηση ληγμένων συνδρομών

### 📅 Κρατήσεις
- Σύστημα κρατήσεων ανά slot
- Καταγραφή παρουσίας/No Show/Ακύρωση
- Έλεγχος 24ωρού για ακυρώσεις
- Κράτηση απευθείας από εβδομαδιαίο πρόγραμμα
- Κάρτα παρουσιών ανά συνδρομή

### 💳 Πληρωμές
- Καταγραφή πληρωμών ανά συνδρομή
- Τρόπος πληρωμής (Μετρητά/Κάρτα/Transfer)
- Καταγραφή απόδειξης
- Ιστορικό πληρωμών ανά μέλος

### 📊 Dashboard & Αναφορές
- Dashboard με στατιστικά
- Διαθεσιμότητα Pilates Reformer (Πρωί/Απόγευμα)
- Ληγούσες συνδρομές (7/14/30 μέρες)
- Αναφορά εσόδων ανά μήνα με Excel export
- Στατιστικά παρουσιών ανά μέλος και πρόγραμμα
- Εβδομαδιαίο πρόγραμμα με κρατήσεις

## Εκκίνηση

### Προαπαιτούμενα
- Visual Studio 2022+
- .NET 10 SDK
- SQL Server Express
- SSMS (προαιρετικό)

### Οδηγίες
1. Clone το repository:
```bash
git clone https://github.com/pvangelatos/GymApp.git
```

2. Άνοιξε το `GymApp.sln` στο Visual Studio

3. Άλλαξε το connection string στο `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=GymAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

4. Τρέξε το Migration στο Package Manager Console:
```powershell
Update-Database
```

5. Εκκίνηση με `F5`

## Αρχιτεκτονική

GymApp/
├── Data/               # AppDbContext
├── Models/             # Domain models
├── Pages/              # Razor Pages
│   ├── Members/        # Διαχείριση μελών
│   ├── Trainers/       # Διαχείριση εκπαιδευτών
│   ├── GymPrograms/    # Διαχείριση προγραμμάτων
│   ├── SubscriptionPlans/ # Πακέτα συνδρομών
│   ├── Subscriptions/  # Συνδρομές
│   ├── TimeSlots/      # Εβδομαδιαίο πρόγραμμα
│   ├── Bookings/       # Κρατήσεις
│   ├── Payments/       # Πληρωμές
│   └── Reports/        # Αναφορές
├── Resources/          # Validation messages (el-GR, en-US)
└── Services/           # Background services

## Developer
**Παναγιώτης Βαγγελάτος**  
Coding Factory @ AUEB — 2026
