using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RDB10_Cerulean_Bee.Models;

namespace RDB10_Cerulean_Bee.Controllers
{
    public class CustomersController : Controller
    {
        private masterEntities db = new masterEntities();

        // GET: Customers
        public ActionResult Index()
        {
            return View(db.Customers.ToList());
        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            foreach (var order in customer.ArtOrders)
            {
                ApparelItem apparel = db.ApparelItems.Find(order.Apparel_Item_Id);
                order.ApparelItem = apparel;
                order.Order_Total = (order.Set_Up_Charges != null ? (float)order.Set_Up_Charges.Value : 0) + 
                                    (order.ApparelItem.Vendor != null ? (order.ApparelItem.Vendor.Apparel_Base_Price != null ? (float)order.ApparelItem.Vendor.Apparel_Base_Price : 0)  : 0)  + 
                                    (order.ApparelItem.Vendor != null ? (order.ApparelItem.Vendor.Apparel_Additional_Charges != null ? (float)order.ApparelItem.Vendor.Apparel_Additional_Charges.Value : 0) : 0) + 
                                    (order.ApparelItem.Vendor != null ? (order.ApparelItem.Vendor.Color_Additional_Charges != null ? (float)order.ApparelItem.Vendor.Color_Additional_Charges.Value : 0) : 0);
                if (order.Customer != null)
                {
                    if (order.Customer.Discount != null)
                    {
                        order.Order_Total -= (float)(order.Order_Total * order.Customer.Discount / 100); 
                    }
                }
            }
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET: Customers/Create
        public ActionResult CreateArtOrder(int id)
        {
            ArtOrder order = new ArtOrder();
            order.Order_Date = DateTime.Now.Date;
            order.customerDetails = db.Customers.Find(id);
            order.Customer_Id = id;
            ApparelItem apparel = new ApparelItem();
            order.ApparelItem = apparel;
            List<ArtLocation> locations = new List<ArtLocation>();
            for (int i=0; i < 5; i++)
            {
                locations.Add(new ArtLocation());
            }
            order.ApparelItem.locations = locations;
            List<SelectListItem> apparels = new List<SelectListItem>();
            apparels.Add(new SelectListItem { Text = "T-Shirt", Value = "T-Shirt" });
            apparels.Add(new SelectListItem { Text = "Shirt", Value = "Shirt" });
            order.apparels = apparels;
            Vendor vendor = new Vendor();
            order.ApparelItem.Vendor = vendor;
            ViewBag.Vendor_Id = db.Vendors.Select(x => x.Vendor_Name).Distinct();
            ViewBag.Vendor_Apparel_Size_Id = db.ApparelSizes.ToList();
            
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateArtOrder(ArtOrder order)
        {
            if (ModelState.IsValid)
            {
                order.ApparelItem.Vendor_Id = db.Vendors.Where(x => x.Vendor_Name.ToLower() == order.ApparelItem.Vendor.Vendor_Name.ToLower() && x.Apparel_Size_Id == order.ApparelItem.Vendor.Apparel_Size_Id).FirstOrDefault().Vendor_Id;
                db.ArtOrders.Add(order);
                db.SaveChanges();
                foreach (ArtLocation location in order.ApparelItem.locations)
                {
                    location.Apparel_Item_Id = order.Apparel_Item_Id.Value;
                    if (location.Art_Location != null)
                    {
                        db.ArtLocations.Add(location);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Customer_Id,Customer_Name,Phone_Number,E_mail,Discount")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Customer_Id,Customer_Name,Phone_Number,E_mail,Discount")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            db.Customers.Remove(customer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
