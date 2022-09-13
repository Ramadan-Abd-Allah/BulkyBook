using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        public DatabaseFacade DataBase { get; set; }
        public ICategoryRepository Category { get;  }
        public ICoverTypeRepository CoverType { get; }

        public IProductRepository Product { get; }

        public ICompanyRepository Company { get; }

        public IApplicationUserRepository ApplicationUser { get; }

        public IShoppingCartRepository  ShoppingCart { get; }   

        public IOrderHeaderRepository OrderHeader { get; }

        public IOrderDetailRepository OrderDetail { get; }
        public void Save();
    }
}
