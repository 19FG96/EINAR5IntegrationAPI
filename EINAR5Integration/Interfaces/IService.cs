using EINAR5Integration.models;
using Microsoft.AspNetCore.Mvc;

namespace EINAR5Integration.Interfaces
{
    public interface IService
    {
        public (bool, string) Login( AuthRequest request);
        public IActionResult GetEvents( EventRequest request);
    }
}
