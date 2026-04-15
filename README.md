# 🚀 Order Flow System (FIX Protocol)

Sistema completo de geração e acumulação de ordens financeiras utilizando **FIX 4.4**, com arquitetura desacoplada e interface web interativa.

---

## 🧠 Visão Geral

O projeto é composto por **3 aplicações principais**:

### 🔹 OrderGenerator API

Responsável por receber requisições HTTP e enviar ordens via protocolo FIX.

### 🔹 OrderAccumulator (Worker)

Processa as ordens recebidas e calcula a **exposição financeira por símbolo**.

### 🔹 UI (Blazor Server)

Interface web para envio de ordens e visualização em tempo real da exposição.

---

## 🏗️ Arquitetura

```
[ UI (Blazor) ]
        ↓
[ OrderGenerator API ]
        ↓
[ FIX Client ]
        ↓
[ OrderAccumulator Worker ]
        ↓
[ Exposure Service ]
```

### Princípios utilizados:

* Separação de responsabilidades
* Arquitetura em camadas (Domain, Application, Infrastructure)
* Comunicação via HTTP + FIX
* Processamento assíncrono
* Estado em memória thread-safe (`ConcurrentDictionary`)

---

## 📡 Comunicação FIX

* Protocolo: **FIX 4.4**
* Biblioteca: [QuickFIX/n](https://quickfixn.org/)
* Modelo: `NewOrderSingle`

---

## 📊 Regras de Negócio

### Ordem

* **Símbolo:** PETR4, VALE3, VIIA4
* **Side:** Buy / Sell
* **Quantidade:** inteiro positivo < 100.000
* **Preço:** decimal positivo entre 0.01 e 999.99

---

### Exposição

```
Exposure = Σ (buy orders) - Σ (sell orders)

Onde:
value = price × quantity
```

* Buy → aumenta exposição
* Sell → diminui exposição

---

## 💻 Funcionalidades

### UI

* Envio de ordens
* Validação de entrada (UX)
* Feedback de erro/sucesso
* Atualização automática (polling)
* Destaque visual de variação de exposição
* Badge BUY/SELL
* Layout responsivo

---

### Backend

* API REST para envio de ordens
* Integração com FIX
* Worker para processamento contínuo
* Cálculo de exposição em tempo real

---

## 🛠️ Tecnologias

* .NET 8
* ASP.NET Core
* Blazor Server
* QuickFIX/n
* FluentValidation
* C#

---

## ▶️ Como Executar

### 1. Clonar o repositório

```bash
git clone <repo-url>
```

---

### 2. Rodar o Worker (OrderAccumulator)

```bash
cd OrderAccumulator.Worker
dotnet run
```

---

### 3. Rodar a API

```bash
cd OrderGenerator.Api
dotnet run
```

* Swagger disponível em:

```
https://localhost:5001
```

---

### 4. Rodar a UI

```bash
cd UI
dotnet run
```

* Acesse:

```
https://localhost:7202
```

---

## ⚠️ Observações

* O Worker expõe endpoint HTTP para consulta de exposição (`/exposure`)
* A API atua como gateway entre UI e FIX
* Comunicação entre serviços via HTTP local

---

## 💡 Decisões Técnicas

* Uso de `ConcurrentDictionary` para garantir thread safety
* Separação entre API e Worker para simular ambiente distribuído
* Validação centralizada no domínio
* UI com validação leve para melhor experiência do usuário
* Tratamento de erros propagado do backend para frontend

---

## 🚀 Diferenciais

* Arquitetura desacoplada
* Integração real com protocolo FIX
* UI interativa com feedback em tempo real
* Animações e UX inspiradas em sistemas financeiros
* Código organizado e extensível

---

## 📌 Possíveis Melhorias

* Testes automatizados (unitários e integração)
* WebSockets em vez de polling
* Persistência em banco de dados
* Autenticação/autorização
* Dashboard com gráficos

---

## 👩‍💻 Autor

Desenvolvido por **Lorrayne Magalhães**

---
