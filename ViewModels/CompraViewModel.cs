using SODERIA_I.Models;
using System.ComponentModel.DataAnnotations;

namespace SODERIA_I.ViewModels
{
    public class CompraViewModel
    {
        public Compra Compra { get; set; }
        public List<Cliente> Clientes { get; set; } // Lista de clientes
        public List<dynamic> TiposCompra { get; set; } // Lista de tipos de compra con precios
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }
        public List<Compra> ComprasDelUltimoMes { get; set; }
        public List<CompraPorTipoViewModel> ComprasPorTipo { get; set; }

        // Nuevas propiedades
        public int? ClienteIdSeleccionado { get; set; }
        public string ZonaSeleccionada { get; set; }

        public class CompraPorTipoViewModel
        {
            public int TipoCompraId { get; set; }
            public string TipoCompraNombre { get; set; }
            public decimal CantidadTotal { get; set; }
        }
    }
}
