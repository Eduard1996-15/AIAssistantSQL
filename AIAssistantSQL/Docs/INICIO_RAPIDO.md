# ?? Inicio Rápido - AI Assistant SQL

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

**Opción A: Solo CodeLlama (Recomendado para empezar)**

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

**Opción B: Todos los modelos recomendados**

#### Windows:
```cmd
install-models.bat
```

#### Linux/Mac:
```bash
chmod +x install-models.sh
./install-models.sh
```

**Opción C: Manual**
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

### **Paso 4: Ejecutar la Aplicación**

Abre **otra terminal** y ejecuta:
```bash
cd AIAssistantSQL
dotnet run
```

Abre tu navegador en: **http://localhost:5000**

---

## ?? **Configurar Base de Datos**

### **1. Ve a "Configuración BD"**

### **2. Ingresa tu cadena de conexión**

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

? **¡Listo!** El sistema extrae automáticamente:
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
"cuántos usuarios activos hay"
```

### **3. La IA genera el SQL:**
```sql
SELECT COUNT(*) FROM Usuario WHERE activo = 1
```

### **4. Se ejecuta automáticamente y muestra:**
- ?? Respuesta interpretada por la IA
- ?? Tabla de datos (alternable)
- ?? SQL generado (colapsable)

---

## ?? **Ejemplos de Consultas**

### **Consultas Simples:**
```
? "cuántos usuarios hay"
? "muestra todos los documentos"
? "últimos 10 registros de logs"
```

### **Consultas con Filtros:**
```
? "usuarios activos"
? "documentos aprobados"
? "logs del último día"
```

### **Consultas con Agregaciones:**
```
? "cuántas líneas tiene cada documento"
? "documentos con más de 15 líneas"
? "usuarios con más de 3 roles"
```

### **Consultas con JOINs:**
```
? "usuarios con sus roles"
? "documentos con sus revisiones"
? "logs con información del usuario"
```

---

## ?? **Troubleshooting**

### **? "Ollama no está disponible"**

**Problema:** Ollama no está corriendo.

**Solución:**
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

**Solución:**
1. Ve a "Configuración BD"
2. Ingresa cadena de conexión
3. Click "Conectar y Extraer Esquema"

---

### **? "SQL generado es incorrecto"**

**Problema:** Estás usando llama3.2 (no recomendado).

**Solución:**
1. Ve a "Configuración"
2. Verifica modelo actual
3. Si es llama3.2, instala CodeLlama:
```bash
ollama pull codellama
```
4. Cambia a CodeLlama en "Configuración"

---

### **? "Invalid column name 'xxx'"**

**Problema:** La IA generó un nombre de columna incorrecto.

**Solución:**
1. Ve a "Diagnóstico" para ver las columnas exactas
2. El sistema intentará autocorregir automáticamente
3. Si persiste, sé más específico:
   ```
   ? "muestra nombres"
   ? "SELECT Name, UserName FROM Usuario"
   ```

---

## ?? **Documentación Adicional**

- [?? MODELOS_IA.md](MODELOS_IA.md) - Guía completa de modelos
- [?? GUIA_DIAGNOSTICO.md](GUIA_DIAGNOSTICO.md) - Solución de problemas
- [? POR_QUE_CODELLAMA.md](POR_QUE_CODELLAMA.md) - Por qué usar CodeLlama
- [?? RESUMEN_PROYECTO.md](RESUMEN_PROYECTO.md) - Arquitectura técnica

---

## ?? **Siguientes Pasos**

### **1. Prueba Consultas Complejas**
```
"documentos con más de 15 líneas ordenados por cantidad"
"usuarios que tienen el rol de administrador"
"logs de errores agrupados por usuario"
```

### **2. Exporta tu Esquema**
1. Ve a "Configuración BD"
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
2. Ve a "Configuración"
3. Prueba diferentes modelos
4. Compara resultados

---

## ? **Tips para Mejores Resultados**

### **? Sé Específico**
```
? "usuarios"
? "usuarios activos ordenados por fecha de creación"
```

### **? Menciona Tablas Explícitamente**
```
? "datos de personas"
? "datos de la tabla Usuario"
```

### **? Usa Términos SQL Cuando Sea Necesario**
```
? "SELECT * FROM Usuario WHERE activo = 1"
? "usuarios con GROUP BY por nivel"
```

### **? Para Agregaciones, Sé Claro**
```
? "documentos con líneas"
? "documentos con más de 15 líneas"
? "cuántas líneas tiene cada documento"
```

---

## ?? **¡Listo para Empezar!**

```bash
# 1. Asegúrate que Ollama está corriendo
ollama serve

# 2. En otra terminal, inicia la app
cd AIAssistantSQL
dotnet run

# 3. Abre tu navegador
# http://localhost:5000

# 4. ¡Empieza a consultar!
```

---

**¡Disfruta consultando bases de datos con lenguaje natural! ??**
