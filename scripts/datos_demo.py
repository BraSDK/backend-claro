"""
Datos de prueba (demo) para SistemaClaro: usuarios, servicios, órdenes de trabajo con fotos,
liquidaciones (algunas ya auditadas) y un Excel de pagos de Claro para probar la auditoría.

Todo pasa por la API (no toca la base directo), así que las reglas del back se respetan.
Requiere el back corriendo. Solo usa la librería estándar de Python
(si está instalado openpyxl, el Excel sale en .xlsx; si no, en .csv).

Uso (desde la carpeta backend-claro):
    python scripts/datos_demo.py                         # crea los datos demo
    python scripts/datos_demo.py --limpiar               # borra SOLO lo que creó este script
    python scripts/datos_demo.py --solo-excel            # rehace el Excel con las SOT demo que siguen pendientes
    python scripts/datos_demo.py --api http://localhost:5112/api

Lo demo se reconoce por: emails @demo.claro, SOT entre 7100000 y 7199999, servicios 9001-9008.
Las credenciales quedan en scripts/salida/usuarios-demo.local.txt (no se sube a git).
"""
import argparse
import json
import random
import struct
import sys
import uuid
import zlib
from datetime import date, timedelta
from pathlib import Path
from urllib import error, request

SALIDA = Path(__file__).parent / "salida"
DOMINIO = "@demo.claro"
PASSWORD = "Demo#2026"           # la misma para todos los usuarios demo (solo pruebas locales)
SOT_BASE = 7100000
random.seed(2026)                # mismos datos en cada ejecución

USUARIOS = [
    # (email, nombre, dni, rol)  rol: 0 ADMIN, 1 TECNICO, 2 ALMACEN
    ("admin" + DOMINIO, "Ana Gutiérrez Medina", "41000001", 0),
    ("almacen" + DOMINIO, "Rosa Paredes Salazar", "41000002", 2),
    ("carlos.quispe" + DOMINIO, "Carlos Quispe Mamani", "42000001", 1),
    ("luis.huaman" + DOMINIO, "Luis Huamán Torres", "42000002", 1),
    ("jorge.condori" + DOMINIO, "Jorge Condori Flores", "42000003", 1),
    ("miguel.rojas" + DOMINIO, "Miguel Rojas Chávez", "42000004", 1),
    ("pedro.vargas" + DOMINIO, "Pedro Vargas Ramos", "42000005", 1),
]

SERVICIOS = [
    # (codigo, nombre, precio, categoria)  categoria: 0 HFC, 1 FTH, 2 MANTTO
    (9001, "INSTALACION HFC", 85.00, 0),
    (9002, "INSTALACION FTTH", 120.00, 1),
    (9003, "TRASLADO EXTERNO", 60.00, 0),
    (9004, "MANTENIMIENTO CORRECTIVO", 45.00, 2),
    (9005, "CAMBIO DE EQUIPO", 35.00, 2),
    (9006, "POSTVENTA FTTH", 55.00, 1),
    (9007, "BAJA DE SERVICIO", 25.00, 2),
    (9008, "INSTALACION DUO HFC", 95.00, 0),
]
TIPO_ORDEN = {0: "HFC", 1: "FTTH", 2: "MANTTO"}

OBSERVACIONES = [
    (0, "Fotos del acta borrosas, pedir nuevas"),
    (0, "Falta foto de la potencia en la roseta"),
    (1, "Conforme según reporte del supervisor"),
    (2, "Cliente reclamó: la instalación quedó incompleta"),
    (2, "SOT duplicada con otra contrata"),
]

TOTAL_ORDENES = 45


# ===== HTTP (urllib) =====
class Api:
    def __init__(self, base):
        self.base = base.rstrip("/") + "/"
        self.token = None

    def llamar(self, metodo, ruta, cuerpo=None, multipart=None, token=None):
        datos, cabeceras = None, {}
        if multipart is not None:
            datos, tipo = multipart
            cabeceras["Content-Type"] = tipo
        elif cuerpo is not None:
            datos = json.dumps(cuerpo).encode()
            cabeceras["Content-Type"] = "application/json"
        if token or self.token:
            cabeceras["Authorization"] = f"Bearer {token or self.token}"
        req = request.Request(self.base + ruta, data=datos, headers=cabeceras, method=metodo)
        try:
            with request.urlopen(req, timeout=60) as r:
                texto = r.read().decode() or "null"
                return r.status, json.loads(texto)
        except error.HTTPError as e:
            texto = e.read().decode()
            try:
                return e.code, json.loads(texto)
            except ValueError:
                return e.code, texto

    def login(self, email):
        estado, res = self.llamar("POST", "Auth/login", {"email": email, "password": PASSWORD})
        if estado != 200:
            sys.exit(f"No se pudo iniciar sesión con {email}: {res}")
        return res["token"]


def multipart(campos, archivos):
    limite = uuid.uuid4().hex
    partes = []
    for nombre, valor in campos.items():
        partes.append(f'--{limite}\r\nContent-Disposition: form-data; name="{nombre}"\r\n\r\n{valor}\r\n'.encode())
    for nombre, (archivo, contenido) in archivos:
        partes.append(
            f'--{limite}\r\nContent-Disposition: form-data; name="{nombre}"; filename="{archivo}"\r\n'
            f"Content-Type: image/png\r\n\r\n".encode() + contenido + b"\r\n"
        )
    partes.append(f"--{limite}--\r\n".encode())
    return b"".join(partes), f"multipart/form-data; boundary={limite}"


def png(ancho=320, alto=240):
    """Foto falsa: franjas de colores (PNG válido hecho a mano)."""
    c1 = [random.randint(60, 220) for _ in range(3)]
    c2 = [random.randint(60, 220) for _ in range(3)]
    filas = b"".join(
        b"\x00" + bytes((c1 if (y // 20) % 2 else c2) * ancho) for y in range(alto)
    )
    def bloque(tipo, datos):
        return struct.pack(">I", len(datos)) + tipo + datos + struct.pack(">I", zlib.crc32(tipo + datos) & 0xFFFFFFFF)
    return (b"\x89PNG\r\n\x1a\n" + bloque(b"IHDR", struct.pack(">IIBBBBB", ancho, alto, 8, 2, 0, 0, 0))
            + bloque(b"IDAT", zlib.compress(filas)) + bloque(b"IEND", b""))


# ===== Crear =====
def crear(api):
    SALIDA.mkdir(exist_ok=True)

    print("1) Usuarios")
    for email, nombre, dni, rol in USUARIOS:
        estado, res = api.llamar("POST", "Auth/register", {
            "email": email, "password": PASSWORD, "nombreCompleto": nombre, "documentoIdentidad": dni, "rol": rol,
        })
        print(f"   {email:<28} {'creado' if estado == 200 else 'ya existía' if 'registrado' in str(res) else res}")

    api.token = api.login(USUARIOS[0][0])
    _, perfil_admin = api.llamar("GET", "Auth/me")

    print("2) Servicios")
    for codigo, nombre, precio, categoria in SERVICIOS:
        estado, res = api.llamar("POST", "Service/create", {"codigo": codigo, "nombre": nombre, "precio": precio, "categoria": categoria})
        print(f"   {codigo} {nombre:<26} {'creado' if estado == 200 else 'ya existía'}")

    print("3) Órdenes de trabajo (cada técnico registra las suyas, con fotos)")
    tecnicos = [u for u in USUARIOS if u[3] == 1]
    tokens = {t[0]: api.login(t[0]) for t in tecnicos}
    ordenes = []
    for i in range(TOTAL_ORDENES):
        sot = SOT_BASE + 1001 + i * 7
        email, nombre, dni, _ = tecnicos[i % len(tecnicos)]
        servicio = random.choice(SERVICIOS)
        estado_campo = random.choices([0, 1, 2], weights=[80, 10, 10])[0]   # COMPLETADO / NO_COMPLETADO / EN_PROGRESO
        fotos = [("imagenes", (f"sot{sot}_{n + 1}.png", png())) for n in range(random.choice([0, 1, 2, 2, 3]))]
        estado, res = api.llamar("POST", "OrdenTrabajo", token=tokens[email], multipart=multipart({
            "Sot": sot, "DocTecnico": dni, "TrabajoRealizado": servicio[1], "EstadoCampo": estado_campo, "UsuarioId": 0,
        }, fotos))
        if estado in (200, 201):
            ordenes.append({"sot": sot, "id": res["ordenTrabajoId"], "tecnico": nombre, "servicio": servicio, "estado": estado_campo})
        elif "Ya existe" not in str(res):
            print(f"   SOT {sot}: {res}")
    print(f"   {len(ordenes)} órdenes nuevas")

    print("4) Liquidaciones (de las órdenes completadas)")
    hoy = date.today()
    liquidadas = []
    for o in ordenes:
        if o["estado"] != 0 or random.random() < 0.1:   # no todas las completadas se liquidan todavía
            continue
        codigo, nombre_serv, precio, categoria = o["servicio"]
        detalles = [{"concepto": "COSTO DE SERVICIO", "cantidad": 1, "precioUnitario": precio, "precioTotal": precio, "tipo": 0}]
        for concepto, prob, maximo in [("DECO", 0.6, 3), ("REPETIDOR", 0.25, 1), ("DROP 150", 0.4, 2), ("DROP 220", 0.2, 1), ("DROP 300", 0.1, 1)]:
            if random.random() < prob:
                detalles.append({"concepto": concepto, "cantidad": random.randint(1, maximo), "precioUnitario": 0, "precioTotal": 0, "tipo": 1})
        obs = [{"textoObservacion": t, "tipo": tipo, "usuarioId": perfil_admin["usuarioId"]}
               for tipo, t in random.sample(OBSERVACIONES, k=random.choice([0, 0, 0, 1]))]
        estado, res = api.llamar("POST", "Liquidacion/crearLiquidacion", {
            "sot": o["sot"], "tipoOrden": TIPO_ORDEN[categoria], "nombreSubTipoOrden": nombre_serv,
            "fechaProgramacion": (hoy - timedelta(days=random.randint(0, 40))).isoformat(),
            "nombreTecnico": o["tecnico"], "estadoSistema": random.choice([0, 1]), "ordenTrabajoId": o["id"],
            "usuarioId": perfil_admin["usuarioId"], "detalles": detalles, "observaciones": obs,
        })
        if estado == 200:
            liquidadas.append({**o, "total": precio})
        else:
            print(f"   SOT {o['sot']}: {res}")
    print(f"   {len(liquidadas)} liquidaciones")

    print("5) Auditoría: la mitad ya auditada a mano, la otra mitad queda para el Excel")
    random.shuffle(liquidadas)
    mitad = len(liquidadas) // 2
    items = []
    for l in liquidadas[:mitad]:
        total = l["total"]
        editado = round(total * 0.9, 2) if random.random() < 0.2 else None   # algunas con precio editado
        cobrar = editado if editado is not None else total
        estado_pago, pagado = random.choices(
            [(1, cobrar), (3, round(cobrar * random.uniform(0.4, 0.9), 2)), (2, 0), (4, None)],
            weights=[55, 20, 10, 15])[0]
        items.append({"sot": l["sot"], "estadoPago": estado_pago, "montoPagado": pagado, "precioTotalEditable": editado})
    estado, res = api.llamar("PUT", "Liquidacion/auditoria", {"items": items})
    print(f"   {res.get('actualizadas', res) if isinstance(res, dict) else res} auditadas")

    print("6) Excel de pagos de Claro (para probar 'Subir Excel de pagos')")
    print(f"   {excel_desde_api(api)}")

    credenciales = SALIDA / "usuarios-demo.local.txt"
    credenciales.write_text(
        "Usuarios demo (solo base local). Misma contraseña para todos: " + PASSWORD + "\n\n"
        + "\n".join(f"{['ADMIN', 'TECNICO', 'ALMACEN'][r]:<8} {e:<28} {n}" for e, n, _, r in USUARIOS) + "\n",
        encoding="utf-8")
    print(f"\nListo. Credenciales en {credenciales}")


def excel_desde_api(api):
    """Arma el Excel con las liquidaciones demo que siguen sin auditar (sirve también al volver a correr)."""
    pendientes, pagina = [], 1
    while True:
        _, res = api.llamar("GET", f"Liquidacion/Listar?canPagina=50&pagina={pagina}&auditada=false")
        lista = res.get("listaGenerica", []) if isinstance(res, dict) else []
        pendientes += [l for l in lista if SOT_BASE <= l["sot"] < SOT_BASE + 100000]
        if len(lista) < 50:
            break
        pagina += 1

    # se reparten los casos en orden para que siempre aparezcan todos
    casos = ["completo", "completo", "dos partes", "incompleto", "completo", "no pago"]
    filas = []
    for i, l in enumerate(sorted(pendientes, key=lambda x: x["sot"])):
        total = float(l.get("precioTotalEditable") or l.get("precioTotalReal") or 0)
        caso = casos[i % len(casos)]
        if caso == "completo":
            filas.append([l["sot"], total])
        elif caso == "dos partes":                                   # Claro pagó en dos líneas: se suman
            filas += [[l["sot"], round(total / 2, 2)], [l["sot"], round(total - round(total / 2, 2), 2)]]
        elif caso == "incompleto":
            filas.append([l["sot"], round(total * 0.6, 2)])
        else:
            filas.append([l["sot"], 0])
    filas += [[SOT_BASE + 99001, 80.0], [SOT_BASE + 99002, 45.5]]   # no existen en el sistema
    return guardar_excel(filas)


def guardar_excel(filas):
    encabezado = ["FECHA PAGO", "NRO SOT", "CLIENTE", "IMPORTE PAGADO"]
    clientes = ["Juan Pérez", "María López", "Comercial Lima SAC", "Rosa Díaz", "Pedro Sánchez"]
    datos = [[(date.today() - timedelta(days=random.randint(0, 10))).strftime("%d/%m/%Y"), sot, random.choice(clientes), monto]
             for sot, monto in filas]
    try:
        import openpyxl
        libro = openpyxl.Workbook()
        hoja = libro.active
        hoja.title = "Pagos Claro"
        hoja.append(encabezado)
        for fila in datos:
            hoja.append(fila)
        ruta = SALIDA / "pagos_claro_demo.xlsx"
        libro.save(ruta)
    except ImportError:
        ruta = SALIDA / "pagos_claro_demo.csv"
        ruta.write_text("\n".join(";".join(map(str, f)) for f in [encabezado] + datos), encoding="utf-8")
    return ruta


# ===== Limpiar =====
def limpiar(api):
    api.token = api.login(USUARIOS[0][0])
    es_demo = lambda sot: SOT_BASE <= int(sot) < SOT_BASE + 100000

    _, res = api.llamar("GET", f"Liquidacion/Listar?canPagina=50&buscar={str(SOT_BASE)[:2]}")
    borradas = 0
    while isinstance(res, dict) and res.get("listaGenerica"):
        demo = [l["sot"] for l in res["listaGenerica"] if es_demo(l["sot"])]
        if not demo:
            break
        for sot in demo:
            borradas += api.llamar("DELETE", f"Liquidacion/{sot}/delete")[0] == 204
        _, res = api.llamar("GET", f"Liquidacion/Listar?canPagina=50&buscar={str(SOT_BASE)[:2]}")
    print(f"Liquidaciones borradas: {borradas}")

    borradas = 0
    for _ in range(20):
        _, res = api.llamar("GET", f"OrdenTrabajo/List?pagina=1&canPagina=50&buscar={str(SOT_BASE)[:2]}")
        demo = [o["ordenTrabajoId"] for o in (res or {}).get("listaGenerica", []) if es_demo(o["sot"])]
        if not demo:
            break
        for oid in demo:
            borradas += api.llamar("DELETE", f"OrdenTrabajo/{oid}/delete")[0] == 204
    print(f"Órdenes borradas: {borradas}")

    for codigo, *_ in SERVICIOS:
        api.llamar("DELETE", f"Service/{codigo}")
    print("Servicios demo borrados")

    _, res = api.llamar("GET", "User/list?cantidadPorPagina=200")
    lista = res.get("datos", []) if isinstance(res, dict) else res
    admin = None
    for u in lista:
        if u["email"].endswith(DOMINIO):
            if u["email"] == USUARIOS[0][0]:
                admin = u
                continue
            estado, r = api.llamar("DELETE", f"User/{u['id']}")
            print(f"   {u['email']:<28} {'borrado' if estado == 200 else r}")
    if admin:   # el admin demo al final, porque es el que tiene la sesión
        estado, r = api.llamar("DELETE", f"User/{admin['id']}")
        print(f"   {admin['email']:<28} {'borrado' if estado == 200 else r}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Datos demo de SistemaClaro")
    parser.add_argument("--api", default="http://localhost:5112/api")
    parser.add_argument("--limpiar", action="store_true", help="borra solo los datos demo")
    parser.add_argument("--solo-excel", action="store_true", help="solo rehace el Excel de pagos")
    args = parser.parse_args()
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")   # tildes bien en la consola de Windows
    api = Api(args.api)
    if args.limpiar:
        limpiar(api)
    elif args.solo_excel:
        SALIDA.mkdir(exist_ok=True)
        api.token = api.login(USUARIOS[0][0])
        print(excel_desde_api(api))
    else:
        crear(api)
