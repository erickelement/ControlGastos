using ControlGastos.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Models
{
    public class TipoCuenta //: IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage="El campo {0} es requerido")] //Se pueden modificar los mensajes de error del modelo que pertenezca
        [PrimeraLetraMayuscula]
        [Display(Name ="Nombre del tipo cuenta")]
        [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas")]//El remote realiza una conexion con el controlador
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Nombre != null && Nombre.Length > 0) 
        //    {
        //        var primeraLetra = Nombre[0].ToString();

        //        if(primeraLetra != primeraLetra.ToUpper()) 
        //        {
        //            yield return new ValidationResult("La primera letra debe ser mayuscula", new[] {nameof(Nombre)});
        //        }
        //    }
        //}
    }
}
