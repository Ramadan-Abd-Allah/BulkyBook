using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o=>o.Id == orderId,includeProperties:"ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(o => o.OrderId == orderId, includeProperties: "Product"),

            };
            return View(orderVM);
        }


        #region APICALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> OrderHeaderList;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(SD.Role_Admin)||User.IsInRole(SD.Role_Employee))
            {
                OrderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                OrderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").Where(
                    o => o.ApplicationUserId == claim.Value);
            }

            switch (status)
            {
                case "pending":
                    OrderHeaderList = OrderHeaderList.Where(o=>o.OrderStatus == SD.StatusPending);
                    break;
                case "inprocess":
                    OrderHeaderList = OrderHeaderList.Where(o => o.OrderStatus == SD.StatusInProcess);
                    break;
                case "approved":
                    OrderHeaderList = OrderHeaderList.Where(o => o.OrderStatus == SD.StatusApproved);
                    break;
                case "completed":
                    OrderHeaderList = OrderHeaderList.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                default:
                    break;

            }
            return Json(new { data = OrderHeaderList });
        }

        #endregion
    }
}
