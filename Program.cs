using System.Diagnostics;
using System.Text;
using VuelosBaratos;

Console.OutputEncoding = Encoding.UTF8;

// 1. Cargar el grafo desde los archivos de texto
var reloj = Stopwatch.StartNew();
var grafo = GrafoVuelos.CargarDesdeArchivos("aeropuertos.txt", "vuelos.txt");
reloj.Stop();

Console.WriteLine("==============================================");
Console.WriteLine("   BUSCADOR DE VUELOS BARATOS - GRAFOS (C#)   ");
Console.WriteLine("==============================================");
Console.WriteLine($"Datos cargados: {grafo.CantidadVertices} aeropuertos y {grafo.CantidadAristas} vuelos");
Console.WriteLine($"Tiempo de carga: {reloj.Elapsed.TotalMilliseconds:0.000} ms");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("----------------- MENÚ -----------------");
    Console.WriteLine("1. Ver aeropuertos (vértices)");
    Console.WriteLine("2. Ver vuelos por aeropuerto (lista de adyacencia)");
    Console.WriteLine("3. Ver matriz de precios");
    Console.WriteLine("4. Buscar la ruta más barata (Dijkstra)");
    Console.WriteLine("5. Buscar la ruta con menos escalas (BFS)");
    Console.WriteLine("6. Vuelos más baratos desde un aeropuerto");
    Console.WriteLine("7. Estadísticas del grafo");
    Console.WriteLine("8. Prueba de tiempo de ejecución");
    Console.WriteLine("0. Salir");
    Console.Write("Opción: ");
    var op = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (op)
    {
        case "1": MostrarAeropuertos(); break;
        case "2": MostrarListaAdyacencia(); break;
        case "3": MostrarMatriz(); break;
        case "4": BuscarRuta(true); break;
        case "5": BuscarRuta(false); break;
        case "6": VuelosBaratosDesde(); break;
        case "7": MostrarEstadisticas(); break;
        case "8": PruebaDeTiempo(); break;
        case "0": Console.WriteLine("¡Buen viaje!"); return;
        default: Console.WriteLine("Opción no válida."); break;
    }
}

// ------------------- REPORTERÍA -------------------

void MostrarAeropuertos()
{
    Console.WriteLine("AEROPUERTOS REGISTRADOS");
    foreach (var a in grafo.Aeropuertos) Console.WriteLine("  " + a);
}

void MostrarListaAdyacencia()
{
    Console.WriteLine("LISTA DE ADYACENCIA (origen -> destino [precio])");
    foreach (var a in grafo.Aeropuertos)
    {
        var destinos = grafo.VuelosDesde(a.Codigo).Select(v => $"{v.Destino}[${v.Precio}]");
        Console.WriteLine($"  {a.Codigo} -> {string.Join(", ", destinos)}");
    }
}

void MostrarMatriz()
{
    var codigos = grafo.Aeropuertos.Select(a => a.Codigo).ToList();
    Console.WriteLine("MATRIZ DE PRECIOS EN USD (fila = origen, columna = destino, - = sin vuelo)");
    Console.Write("     ");
    foreach (var c in codigos) Console.Write($"{c,5}");
    Console.WriteLine();
    foreach (var o in codigos)
    {
        Console.Write($"{o,-5}");
        foreach (var d in codigos)
        {
            var p = grafo.PrecioDirecto(o, d);
            Console.Write(p.HasValue ? $"{p.Value,5:0}" : $"{"-",5}");
        }
        Console.WriteLine();
    }
}

void BuscarRuta(bool porPrecio)
{
    var origen = PedirCodigo("Código de origen: ");
    var destino = PedirCodigo("Código de destino: ");
    if (origen == null || destino == null) return;

    var sw = Stopwatch.StartNew();
    var ruta = porPrecio ? grafo.RutaMasBarata(origen, destino) : grafo.RutaMenosEscalas(origen, destino);
    sw.Stop();

    Console.WriteLine();
    Console.WriteLine(porPrecio ? "RUTA MÁS BARATA (Dijkstra)" : "RUTA CON MENOS ESCALAS (BFS)");
    if (!ruta.Existe)
    {
        Console.WriteLine("  No existe una ruta entre esos aeropuertos.");
    }
    else
    {
        int n = 1;
        foreach (var t in ruta.Tramos) Console.WriteLine($"  Tramo {n++}: {t}");
        Console.WriteLine($"  Total: ${ruta.PrecioTotal:0.00} | Escalas: {ruta.Escalas} | Tiempo en el aire: {ruta.MinutosTotales} min");
    }
    Console.WriteLine($"  Tiempo de búsqueda: {sw.Elapsed.TotalMilliseconds:0.0000} ms");
}

void VuelosBaratosDesde()
{
    var origen = PedirCodigo("Código del aeropuerto: ");
    if (origen == null) return;
    Console.WriteLine($"VUELOS DIRECTOS DESDE {grafo.Obtener(origen).Ciudad}, del más barato al más caro:");
    foreach (var v in grafo.VuelosDesde(origen).OrderBy(v => v.Precio))
        Console.WriteLine("  " + v);
}

void MostrarEstadisticas()
{
    var grados = grafo.Grados();
    Console.WriteLine("ESTADÍSTICAS DEL GRAFO");
    Console.WriteLine($"  Vértices (aeropuertos): {grafo.CantidadVertices}");
    Console.WriteLine($"  Aristas (vuelos):       {grafo.CantidadAristas}");
    double densidad = (double)grafo.CantidadAristas / (grafo.CantidadVertices * (grafo.CantidadVertices - 1));
    Console.WriteLine($"  Densidad:               {densidad:P1}");
    Console.WriteLine("  Aeropuerto  Salidas  Llegadas");
    foreach (var g in grados.OrderByDescending(x => x.Value.salida + x.Value.entrada))
        Console.WriteLine($"  {g.Key,-10} {g.Value.salida,7} {g.Value.entrada,9}");
    var top = grados.MaxBy(x => x.Value.salida + x.Value.entrada);
    Console.WriteLine($"  Aeropuerto más conectado: {grafo.Obtener(top.Key)}");
}

void PruebaDeTiempo()
{
    Console.WriteLine("PRUEBA DE TIEMPO CON GRAFOS ALEATORIOS (cada aeropuerto tiene 5 vuelos)");
    Console.WriteLine($"  {"Vértices",9} {"Aristas",9} {"Dijkstra (ms)",15} {"BFS (ms)",10}");
    var rnd = new Random(42);
    foreach (var n in new[] { 100, 1_000, 10_000, 100_000 })
    {
        var g = new GrafoVuelos();
        for (int i = 0; i < n; i++) g.AgregarAeropuerto(new Aeropuerto("A" + i, "Ciudad " + i, "X"));
        for (int i = 0; i < n; i++)
            for (int k = 0; k < 5; k++)
                g.AgregarVuelo(new Vuelo("A" + i, "A" + rnd.Next(n), rnd.Next(30, 900), "Ficticia", 60));

        const int repeticiones = 10;
        double tDij = 0, tBfs = 0;
        for (int r = 0; r < repeticiones; r++)
        {
            string o = "A" + rnd.Next(n), d = "A" + rnd.Next(n);
            var sw = Stopwatch.StartNew(); g.RutaMasBarata(o, d); sw.Stop(); tDij += sw.Elapsed.TotalMilliseconds;
            sw.Restart(); g.RutaMenosEscalas(o, d); sw.Stop(); tBfs += sw.Elapsed.TotalMilliseconds;
        }
        Console.WriteLine($"  {n,9:N0} {g.CantidadAristas,9:N0} {tDij / repeticiones,15:0.000} {tBfs / repeticiones,10:0.000}");
    }
    Console.WriteLine("  (promedio de 10 búsquedas por tamaño)");
}

string? PedirCodigo(string mensaje)
{
    Console.Write(mensaje);
    var c = Console.ReadLine()?.Trim().ToUpper() ?? "";
    if (grafo.Existe(c)) return c;
    Console.WriteLine($"  El código '{c}' no existe. Use la opción 1 para ver los códigos.");
    return null;
}
