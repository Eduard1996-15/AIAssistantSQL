# ?? Inicio R�pido - AI Assistant SQL

## ? **Setup en 5 Minutos**

### **Paso 1: Instalar Pre-requisitos**

#### **.NET 8 SDK**
```bash
# Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
# O verificar si ya lo tienes:
dotnet --version
# Debe mostrar: 8.x.x
```

#### **Ollama**
```bash
# Descargar desde: https://ollama.ai
# Seguir instrucciones del instalador
```

---

### **Paso 2: Instalar Modelo de IA (IMPORTANTE)**

**Opci�n A: Solo CodeLlama (Recomendado para empezar)**

#### Windows:
```cmd
cd AIAssistantSQL
install-codellama.bat
```

#### Linux/Mac:
```bash
cd AIAssistantSQL
chmod +x install-codellama.sh
./install-codellama.sh
```

**Opci�n B: Todos los modelos recomendados**

#### Windows:
```cmd
install-models.bat
```

#### Linux/Mac:
```bash
chmod +x install-models.sh
./install-models.sh
```

**Opci�n C: Manual**
```bash
ollama pull codellama
```

---

### **Paso 3: Iniciar Ollama**
```bash
ollama serve
```

Deja esta terminal abierta.

---

### **Paso 4: Ejecutar la Aplicaci�n**

Abre **otra terminal** y ejecuta:
```bash
cd AIAssistantSQL
dotnet run
```

Abre tu navegador en: **http://localhost:5000**

---

## ?? **Configurar Base de Datos**

### **1. Ve a "Configuraci�n BD"**

### **2. Ingresa tu cadena de conexi�n**

#### **SQL Server (Windows Authentication):**
```
Server=localhost\SQLEXPRESS;Database=MiDB;Trusted_Connection=True;TrustServerCertificate=True;
```

#### **SQL Server (Usuario/Password):**
```
Server=localhost;Database=MiDB;User Id=sa;Password=TuPassword;TrustServerCertificate=True;
```

#### **PostgreSQL:**
```
Host=localhost;Database=MiDB;Username=postgres;Password=TuPassword;
```

### **3. Click "Conectar y Extraer Esquema Completo"**

? **�Listo!** El sistema extrae autom�ticamente:
- Todas las tablas
- Todas las columnas
- Claves primarias
- Foreign Keys
- Relaciones

---

## ?? **Hacer tu Primera Consulta**

### **1. Ve a "Consultas"**

### **2. Escribe en lenguaje natural:**

```
"cu�ntos usuarios activos hay"
```

### **3. La IA genera el SQL:**
```sql
SELECT COUNT(*) FROM Usuario WHERE activo = 1
```

### **4. Se ejecuta autom�ticamente y muestra:**
- ?? Respuesta interpretada por la IA
- ?? Tabla de datos (alternable)
- ?? SQL generado (colapsable)

---

## ?? **Ejemplos de Consultas**

### **Consultas Simples:**
```
? "cu�ntos usuarios hay"
? "muestra todos los documentos"
? "�ltimos 10 registros de logs"
```

### **Consultas con Filtros:**
```
? "usuarios activos"
? "documentos aprobados"
? "logs del �ltimo d�a"
```

### **Consultas con Agregaciones:**
```
? "cu�ntas l�neas tiene cada documento"
? "documentos con m�s de 15 l�neas"
? "usuarios con m�s de 3 roles"
```

### **Consultas con JOINs:**
```
? "usuarios con sus roles"
? "documentos con sus revisiones"
? "logs con informaci�n del usuario"
```

---

## ?? **Troubleshooting**

### **? "Ollama no est� disponible"**

**Problema:** Ollama no est� corriendo.

**Soluci�n:**
```bash
ollama serve
```

**Verificar:**
```bash
curl http://localhost:11434/api/tags
```

---

### **? "No hay esquema cargado"**

**Problema:** No has conectado a una base de datos.

**Soluci�n:**
1. Ve a "Configuraci�n BD"
2. Ingresa cadena de conexi�n
3. Click "Conectar y Extraer Esquema"

---

### **? "SQL generado es incorrecto"**

**Problema:** Est�s usando llama3.2 (no recomendado).

**Soluci�n:**
1. Ve a "Configuraci�n"
2. Verifica modelo actual
3. Si es llama3.2, instala CodeLlama:
```bash
ollama pull codellama
```
4. Cambia a CodeLlama en "Configuraci�n"

---

### **? "Invalid column name 'xxx'"**

**Problema:** La IA gener� un nombre de columna incorrecto.

**Soluci�n:**
1. Ve a "Diagn�stico" para ver las columnas exactas
2. El sistema intentar� autocorregir autom�ticamente
3. Si persiste, s� m�s espec�fico:
   ```
   ? "muestra nombres"
   ? "SELECT Name, UserName FROM Usuario"
   ```

---

## ?? **Documentaci�n Adicional**

- [?? MODELOS_IA.md](MODELOS_IA.md) - Gu�a completa de modelos
- [?? GUIA_DIAGNOSTICO.md](GUIA_DIAGNOSTICO.md) - Soluci�n de problemas
- [? POR_QUE_CODELLAMA.md](POR_QUE_CODELLAMA.md) - Por qu� usar CodeLlama
- [?? RESUMEN_PROYECTO.md](RESUMEN_PROYECTO.md) - Arquitectura t�cnica

---

## ?? **Siguientes Pasos**

### **1. Prueba Consultas Complejas**
```
"documentos con m�s de 15 l�neas ordenados por cantidad"
"usuarios que tienen el rol de administrador"
"logs de errores agrupados por usuario"
```

### **2. Exporta tu Esquema**
1. Ve a "Configuraci�n BD"
2. Baja hasta "Guardar Esquema Actual"
3. Nombre: `mi_esquema.json`
4. Click "Guardar Esquema"

**Ventajas:**
- ? Reutilizable en otros proyectos
- ? No necesitas la BD para trabajar
- ? Compartible con tu equipo

### **3. Experimenta con Otros Modelos**
1. Instala modelos alternativos:
```bash
ollama pull mistral
ollama pull deepseek-coder
```
2. Ve a "Configuraci�n"
3. Prueba diferentes modelos
4. Compara resultados

---

## ? **Tips para Mejores Resultados**

### **? S� Espec�fico**
```
? "usuarios"
? "usuarios activos ordenados por fecha de creaci�n"
```

### **? Menciona Tablas Expl�citamente**
```
? "datos de personas"
? "datos de la tabla Usuario"
```

### **? Usa T�rminos SQL Cuando Sea Necesario**
```
? "SELECT * FROM Usuario WHERE activo = 1"
? "usuarios con GROUP BY por nivel"
```

### **? Para Agregaciones, S� Claro**
```
? "documentos con l�neas"
? "documentos con m�s de 15 l�neas"
? "cu�ntas l�neas tiene cada documento"
```

---

## ?? **�Listo para Empezar!**

```bash
# 1. Aseg�rate que Ollama est� corriendo
ollama serve

# 2. En otra terminal, inicia la app
cd AIAssistantSQL
dotnet run

# 3. Abre tu navegador
# http://localhost:5000

# 4. �Empieza a consultar!
```

---

**�Disfruta consultando bases de datos con lenguaje natural! ??**
