using System.Globalization;

namespace VuelosBaratos;

// Grafo dirigido y ponderado representado con LISTA DE ADYACENCIA.
// Clave: código del aeropuerto. Valor: lista de vuelos que salen de él.
public class GrafoVuelos
{
    private readonly Dictionary<string, Aeropuerto> aeropuertos = new();
    private readonly Dictionary<string, List<Vuelo>> adyacencia = new();

    public int CantidadVertices => aeropuertos.Count;
    public int CantidadAristas => adyacencia.Values.Sum(l => l.Count);
    public IEnumerable<Aeropuerto> Aeropuertos => aeropuertos.Values;

    public bool Existe(string codigo) => aeropuertos.ContainsKey(codigo);
    public Aeropuerto Obtener(string codigo) => aeropuertos[codigo];

    public void AgregarAeropuerto(Aeropuerto a)
    {
        if (aeropuertos.ContainsKey(a.Codigo)) return;
        aeropuertos[a.Codigo] = a;
        adyacencia[a.Codigo] = new List<Vuelo>();
    }

    public void AgregarVuelo(Vuelo v)
    {
        if (!Existe(v.Origen) || !Existe(v.Destino))
            throw new ArgumentException($"Aeropuerto desconocido en el vuelo {v.Origen}->{v.Destino}");
        adyacencia[v.Origen].Add(v);
    }

    public List<Vuelo> VuelosDesde(string codigo) => adyacencia[codigo];

    // ---------- Carga desde archivos de texto (bloc de notas) ----------
    public static GrafoVuelos CargarDesdeArchivos(string rutaAeropuertos, string rutaVuelos)
    {
        var g = new GrafoVuelos();

        foreach (var linea in File.ReadAllLines(rutaAeropuertos))
        {
            if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith('#')) continue;
            var p = linea.Split(';');
            g.AgregarAeropuerto(new Aeropuerto(p[0].Trim(), p[1].Trim(), p[2].Trim()));
        }

        foreach (var linea in File.ReadAllLines(rutaVuelos))
        {
            if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith('#')) continue;
            var p = linea.Split(';');
            g.AgregarVuelo(new Vuelo(
                p[0].Trim(), p[1].Trim(),
                decimal.Parse(p[2], CultureInfo.InvariantCulture),
                p[3].Trim(), int.Parse(p[4])));
        }
        return g;
    }

    // ---------- Dijkstra: ruta más BARATA ----------
    // Usa una cola de prioridad. Complejidad: O((V + E) log V)
    public Ruta RutaMasBarata(string origen, string destino)
    {
        var costo = new Dictionary<string, decimal>();
        var anterior = new Dictionary<string, Vuelo?>();
        foreach (var c in aeropuertos.Keys) { costo[c] = decimal.MaxValue; anterior[c] = null; }

        var cola = new PriorityQueue<string, decimal>();
        costo[origen] = 0;
        cola.Enqueue(origen, 0);

        while (cola.TryDequeue(out var actual, out var costoActual))
        {
            if (costoActual > costo[actual]) continue;   // entrada vieja, se ignora
            if (actual == destino) break;                // ya llegamos

            foreach (var vuelo in adyacencia[actual])
            {
                var nuevo = costo[actual] + vuelo.Precio;
                if (nuevo < costo[vuelo.Destino])
                {
                    costo[vuelo.Destino] = nuevo;
                    anterior[vuelo.Destino] = vuelo;
                    cola.Enqueue(vuelo.Destino, nuevo);
                }
            }
        }
        return Reconstruir(anterior, origen, destino);
    }

    // ---------- BFS: ruta con MENOS ESCALAS ----------
    // Complejidad: O(V + E)
    public Ruta RutaMenosEscalas(string origen, string destino)
    {
        var anterior = new Dictionary<string, Vuelo?> { [origen] = null };
        var cola = new Queue<string>();
        cola.Enqueue(origen);

        while (cola.Count > 0)
        {
            var actual = cola.Dequeue();
            if (actual == destino) break;
            foreach (var vuelo in adyacencia[actual])
            {
                if (anterior.ContainsKey(vuelo.Destino)) continue;
                anterior[vuelo.Destino] = vuelo;
                cola.Enqueue(vuelo.Destino);
            }
        }
        return Reconstruir(anterior, origen, destino);
    }

    private static Ruta Reconstruir(Dictionary<string, Vuelo?> anterior, string origen, string destino)
    {
        var ruta = new Ruta();
        if (origen == destino || !anterior.ContainsKey(destino) || anterior[destino] == null) return ruta;
        var pila = new Stack<Vuelo>();
        var paso = destino;
        while (paso != origen)
        {
            var v = anterior[paso]!;
            pila.Push(v);
            paso = v.Origen;
        }
        ruta.Tramos.AddRange(pila);
        return ruta;
    }

    // Grado de salida y de entrada de cada aeropuerto (conexiones).
    public Dictionary<string, (int salida, int entrada)> Grados()
    {
        var r = aeropuertos.Keys.ToDictionary(k => k, k => (salida: adyacencia[k].Count, entrada: 0));
        foreach (var lista in adyacencia.Values)
            foreach (var v in lista)
                r[v.Destino] = (r[v.Destino].salida, r[v.Destino].entrada + 1);
        return r;
    }

    // Precio directo entre dos aeropuertos (para la matriz). null si no hay vuelo.
    public decimal? PrecioDirecto(string o, string d) =>
        adyacencia[o].Where(v => v.Destino == d).Select(v => (decimal?)v.Precio).Min();
}
