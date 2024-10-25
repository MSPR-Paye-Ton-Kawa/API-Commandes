# PS-2024-MSPR-Paye-Ton-Kawa

## 📚 Projet Scolaire | MSPR

Juin-Septembre 2024

Groupe : Juliette, Flavien, Yasmine & Colas

### 📌 Consignes du projet : 

CERTIFICATION PROFESSIONNELLE EXPERT EN INFORMATIQUE ET SYSTEME D’INFORMATION

BLOC 4 – Concevoir et développer des solutions applicatives métier et spécifiques (mobiles, embarquées et ERP)

Cahier des Charges de la MSPR « Conception d’une solution applicative en adéquation avec l’environnement technique étudié


### 🐱 Notre projet :

Ce repos est destiné à l'API Commandes.

📦 Table Orders :

- OrderId (int, Primary Key, Auto-increment) : Identifiant unique de la commande.

- CustomerId (int, Foreign Key, not null) : Identifiant du client associé à la commande (géré de manière externe).

- Date (datetime, not null) : Date de la commande.

- Status (varchar, not null) : Statut de la commande.

- OrderItems (ICollection<OrderItem>) : Collection des articles de la commande.

- Payments (ICollection<Payment>) : Collection des paiements associés à la commande.


📦 Table OrderItems :

- OrderItemId (int, Primary Key, Auto-increment) : Identifiant unique de l'article de la commande.

- OrderId (int, Foreign Key, not null) : Identifiant de la commande associée.

- ProductId (int, Foreign Key, not null) : Identifiant du produit (géré de manière externe).

- Quantity (int, not null) : Quantité de l'article commandé.


📦 Table Payments :

- PaymentId (int, Primary Key, Auto-increment) : Identifiant unique du paiement.

- OrderId (int, Foreign Key, not null) : Identifiant de la commande associée.

- Amount (decimal, not null) : Montant du paiement.

- PaymentDate (datetime, not null) : Date du paiement.

- PaymentMethod (varchar, not null) : Méthode de paiement utilisée.

- Status (varchar, not null) : Statut du paiement.


Commandes Docker :

docker build -t apicommandes:latest .

docker run -d -p 8080:80 --name apicommandes apicommandes:latest


### 📎 Branches :

- main : Solution finale, prod.
  
- dev : Solution fonctionnelle en dev.
  
- hotfix : Correction de bugs et autres.

- release : Solution fonctionnelle de dev à prod.

- feature-db : Développement lié à la base de données.

- feature-tests : Développement des tests.

- feature-messagebroker : Développement de la partie message broker.

- feature-owasp-dependency-check : Développement de la partie sécurité.

- feature-docker : Développement de la partie Docker.

- bugfix-* : Correction de bugs.


### 💻 Applications et langages utilisés :

- C#
- Visual Studio
- Docker



## 🌸 Merci !
© J-IFT
