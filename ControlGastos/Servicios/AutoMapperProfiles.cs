using AutoMapper;
using ControlGastos.Models;

namespace ControlGastos.Servicios
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
        }
    }
}
