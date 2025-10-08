# ?? Gu�a de Modelos de IA para SQL

## ?? **Modelo por Defecto: CodeLlama**

El proyecto viene configurado con **CodeLlama** como modelo por defecto porque es el MEJOR para generar consultas SQL.

---

## ?? **Comparaci�n de Modelos:**

| Modelo | Tama�o | Especialidad | Precisi�n SQL | Velocidad | Recomendado Para |
|--------|--------|--------------|---------------|-----------|------------------|
| **codellama** | 3.8 GB | C�digo/SQL | ????? | ??? | **SQL complejo** ? |
| **mistral** | 4.1 GB | General | ???? | ???? | Razonamiento + SQL |
| **deepseek-coder** | 3.8 GB | C�digo | ????? | ??? | SQL muy complejo |
| llama3.2 | 2.0 GB | General | ?? | ????? | ? NO recomendado |
| llama3 | 4.7 GB | General | ??? | ??? | Consultas simples |
| phi3 | 2.3 GB | Peque�o | ??? | ????? | SQL b�sico |

---

## ?? **Instalaci�n R�pida:**

### **Windows:**
```cmd
install-models.bat
```

### **Linux/Mac:**
```bash
chmod +x install-models.sh
./install-models.sh
```

Este script instalar� autom�ticamente:
1. ? **CodeLlama** (3.8 GB) - Modelo principal
2. ? **Mistral** (4.1 GB) - Alternativa balanceada
3. ? **DeepSeek Coder** (3.8 GB) - Para SQL muy complejo

**Espacio total:** ~12 GB

---

## ?? **Modelos Recomendados:**

### **1. CodeLlama (Por Defecto) ?????**

```bash
ollama pull codellama
```

**Ventajas:**
- ? Especializado en c�digo y SQL
- ? Entiende GROUP BY, HAVING, JOINs
- ? Comprende relaciones entre tablas
- ? Genera SQL idiom�tico y eficiente
- ? Pocas alucinaciones

**Ideal para:**
- Consultas con agregaciones (COUNT, SUM, AVG)
- JOINs complejos
- Subqueries
- Filtros con HAVING
- Relaciones Foreign Key

**Ejemplo:**
```
Pregunta: "documentos con m�s de 15 l�neas"
SQL: SELECT d.DocumentoId, COUNT(*) 
     FROM DOCUMENTO d 
     JOIN lineas_documento l ON d.DocumentoId = l.DocumentoId 
     GROUP BY d.DocumentoId 
     HAVING COUNT(*) > 15
? CORRECTO
```

---

### **2. Mistral ????**

```bash
ollama pull mistral
```

**Ventajas:**
- ? Buen balance entre SQL y razonamiento
- ? R�pido y eficiente
- ? Bueno para consultas medianas
- ? Menos especializado pero vers�til

**Ideal para:**
- Consultas con contexto complejo
- Interpretaci�n de resultados
- SQL con l�gica de negocio
- Cuando necesitas explicaciones

**Desventajas:**
- ?? A veces genera SQL m�s verboso
- ?? Puede sobre-complicar consultas simples

---

### **3. DeepSeek Coder ?????**

```bash
ollama pull deepseek-coder
```

**Ventajas:**
- ? MUY preciso para SQL complejo
- ? Especializado en programaci�n
- ? Excelente con subqueries
- ? Entiende patrones avanzados

**Ideal para:**
- SQL extremadamente complejo
- CTEs (WITH clauses)
- Window functions
- Queries anidadas
- Optimizaci�n de consultas

**Desventajas:**
- ?? Puede ser m�s lento
- ?? A veces sobre-optimiza consultas simples

---

## ? **Modelos NO Recomendados:**

### **llama3.2 (2 GB)**

```bash
# NO usar para SQL complejo
ollama pull llama3.2
```

**Por qu� NO:**
- ? NO entiende GROUP BY / HAVING
- ? Confunde "cantidad" con "columna"
- ? NO comprende relaciones FK
- ? Genera SQL incorrecto en 90% de casos complejos

**Solo usar para:**
- Consultas MUY simples (`SELECT * FROM tabla`)
- Pruebas r�pidas
- Cuando la velocidad es cr�tica

---

## ?? **C�mo Cambiar de Modelo:**

### **Opci�n 1: Interfaz Web (Recomendado)**
1. Abre http://localhost:5000
2. Ve a **"Configuraci�n"**
3. Selecciona el modelo deseado
4. Click **"Usar este Modelo"**

### **Opci�n 2: appsettings.json (Permanente)**
```json
{
  "Ollama": {
    "Url": "http://localhost:11434",
    "Model": "codellama"  ? Cambia aqu�
  }
}
```

Modelos disponibles:
- `codellama` (por defecto)
- `mistral`
- `deepseek-coder`
- `llama3.2` (no recomendado)

---

## ?? **Matriz de Decisi�n:**

```
???????????????????????????????????????????????????
? Tu consulta tiene:                              ?
???????????????????????????????????????????????????
? ? GROUP BY / HAVING        ? codellama         ?
? ? JOINs m�ltiples          ? codellama         ?
? ? Agregaciones (COUNT/SUM) ? codellama         ?
? ? Subqueries complejas     ? deepseek-coder    ?
? ? CTEs (WITH)              ? deepseek-coder    ?
? ? Window functions         ? deepseek-coder    ?
? ? Razonamiento + SQL       ? mistral           ?
? ? SELECT simple            ? cualquiera        ?
???????????????????????????????????????????????????
```

---

## ?? **Prueba de Rendimiento:**

### **Test: "documentos con m�s de 15 l�neas"**

| Modelo | SQL Generado | Correcto |
|--------|--------------|----------|
| **codellama** | `GROUP BY DocumentoId HAVING COUNT(*) > 15` | ? |
| **mistral** | `GROUP BY DocumentoId HAVING COUNT(*) > 15` | ? |
| **deepseek-coder** | `GROUP BY DocumentoId HAVING COUNT(*) > 15` | ? |
| llama3.2 | `WHERE Nivel = 15` | ? |

### **Test: "usuarios con sus roles (JOIN)"**

| Modelo | Usa JOIN | Correcto |
|--------|----------|----------|
| **codellama** | ? | ? |
| **mistral** | ? | ? |
| **deepseek-coder** | ? | ? |
| llama3.2 | ? | ? |

---

## ?? **Recomendaci�n Final:**

```
???????????????????????????????????????????????????
?  PARA ESTE PROYECTO:                            ?
?                                                 ?
?  ?? Mejor opci�n: codellama (por defecto)      ?
?  ?? Alternativa: mistral                        ?
?  ?? SQL avanzado: deepseek-coder                ?
?  ? Evitar: llama3.2                            ?
???????????????????????????????????????????????????
```

---

## ?? **Inicio R�pido:**

```bash
# 1. Instalar modelos recomendados
install-models.bat  # Windows
# o
./install-models.sh  # Linux/Mac

# 2. Iniciar Ollama
ollama serve

# 3. Verificar modelos instalados
ollama list

# 4. Iniciar la aplicaci�n
dotnet run

# 5. �Listo! El modelo por defecto es codellama
```

---

## ?? **Soporte:**

Si alg�n modelo no funciona bien:
1. Verifica que Ollama est� corriendo: `ollama serve`
2. Reinstala el modelo: `ollama pull codellama`
3. Cambia de modelo en "Configuraci�n"
4. Revisa los logs en la consola

---

**�Disfruta generando consultas SQL con IA! ??**
