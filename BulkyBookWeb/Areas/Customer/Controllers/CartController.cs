using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM shoppingCartVM;


        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(
                s => s.ApplicationUserId == claim.Value, includeProperties: "Product").ToList(),
                OrderHeader = new()
            };
            foreach (var item in shoppingCartVM.ListCart)
            {
                item.Price = _unitOfWork.Product.GetPriceBasedOnQuantity(
                    item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(shoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cartFromDb, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cartFromDb, 1);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(
                s => s.ApplicationUserId == claim.Value, includeProperties: "Product").ToList(),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var item in shoppingCartVM.ListCart)
            {
                item.Price = _unitOfWork.Product.GetPriceBasedOnQuantity(
                    item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ShoppingCartVM shoppingCartVM)
        {
            using (var transaction = _unitOfWork.DataBase.BeginTransaction())
            {
                try
                {

                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                    shoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(
                        s => s.ApplicationUserId == claim.Value, includeProperties: "Product").ToList();

                    ApplicationUser applicationUserFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                    
                    foreach (var item in shoppingCartVM.ListCart)
                    {
                        item.Price = _unitOfWork.Product.GetPriceBasedOnQuantity(
                            item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                        shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
                    }
                    if (applicationUserFromDb.CompanyId.GetValueOrDefault() == 0)
                    {
                        shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                        shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                    }
                    else
                    {
                        shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                        shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                    }
                    shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                    shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

                    _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
                    _unitOfWork.Save();
                    foreach (var cartItem in shoppingCartVM.ListCart)
                    {
                        OrderDetail orderDetail = new()
                        {
                            ProductId = cartItem.ProductId,
                            OrderId = shoppingCartVM.OrderHeader.Id,
                            Price = cartItem.Price,
                            Count = cartItem.Count,
                        };
                        _unitOfWork.OrderDetail.Add(orderDetail);
                        _unitOfWork.Save();
                    }

                    if (applicationUserFromDb.CompanyId.GetValueOrDefault() == 0)
                    {
                        // Stripe Settings
                        var domain = "https://localhost:7129/";
                        var options = new SessionCreateOptions
                        {
                            PaymentMethodTypes = new List<string>
                        {
                            "card",
                        },
                            LineItems = new List<SessionLineItemOptions>(),
                            Mode = "payment",
                            SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                            CancelUrl = domain + $"Customer/Cart/Index",
                        };
                        foreach (var item in shoppingCartVM.ListCart)
                        {

                            var sessionLineItem = new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    UnitAmount = (long?)(item.Price * 100),
                                    Currency = "usd",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = item.Product.Title,
                                    },

                                },
                                Quantity = item.Count,
                            };
                            options.LineItems.Add(sessionLineItem);
                        }

                        var service = new SessionService();
                        Session session = service.Create(options);
                        _unitOfWork.OrderHeader.UpdateStripePayment(
                            shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                        _unitOfWork.Save();
                        Response.Headers.Add("Location", session.Url);
                        transaction.Commit();
                        return new StatusCodeResult(303);
                    }
                    else
                    {
                        transaction.Commit();
                        return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartVM.OrderHeader.Id });
                    }
                    
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return RedirectToAction("Index", "Home");
                }
            }
            //return RedirectToAction("Index", "Home");
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            if (orderHeaderFromDb.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeaderFromDb.SessionId);
                // Check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            
            List<ShoppingCart> shoppingCart = _unitOfWork.ShoppingCart.GetAll(
                o => o.ApplicationUserId == orderHeaderFromDb.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCart);
            _unitOfWork.Save();
             return View(id);
        }
    }
}
