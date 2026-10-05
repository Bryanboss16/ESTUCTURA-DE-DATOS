namespace VuelosBaratos;

// Un aeropuerto es un VÉRTICE (nodo) del grafo.
public class Aeropuerto
{
    public string Codigo { get; }
    public string Ciudad { get; }
    public string Pais { get; }

    public Aeropuerto(string codigo, string ciudad, string pais)
    {
        Codigo = codigo;
        Ciudad = ciudad;
        Pais = pais;
    }

    public override string ToString() => $"{Codigo} - {Ciudad} ({Pais})";
}

// Un vuelo es una ARISTA dirigida y con peso (el precio) entre dos aeropuertos.
public class Vuelo
{
    public string Origen { get; }
    public string Destino { get; }
    public decimal Precio { get; }
    public string Aerolinea { get; }
    public int Minutos { get; }

    public Vuelo(string origen, string destino, decimal precio, string aerolinea, int minutos)
    {
        Origen = origen;
        Destino = destino;
        Precio = precio;
        Aerolinea = aerolinea;
        Minutos = minutos;
    }

    public override string ToString() =>
        $"{Origen} -> {Destino}  ${Precio,6:0.00}  {Aerolinea,-11} {Minutos,4} min";
}

// Resultado de una búsqueda de ruta.
public class Ruta
{
    public List<Vuelo> Tramos { get; } = new();
    public decimal PrecioTotal => Tramos.Sum(t => t.Precio);
    public int MinutosTotales => Tramos.Sum(t => t.Minutos);
    public int Escalas => Math.Max(0, Tramos.Count - 1);
    public bool Existe => Tramos.Count > 0;
}
