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
    public class ArtOrdersController : Controller
    {
        private masterEntities db = new masterEntities();

        // GET: ArtOrders
        public ActionResult Index()
        {
            var artOrders = db.ArtOrders.Include(a => a.ApparelItem).Include(a => a.Customer);
            return View(artOrders.ToList());
        }

        // GET: ArtOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArtOrder artOrder = db.ArtOrders.Find(id);
            if (artOrder == null)
            {
                return HttpNotFound();
            }
            return View(artOrder);
        }

        // GET: ArtOrders/Create
        public ActionResult Create()
        {
            ViewBag.Apparel_Item_Id = new SelectList(db.ApparelItems, "Apparel_Item_Id", "Base_Color");
            ViewBag.Customer_Id = new SelectList(db.Customers, "Customer_Id", "Customer_Name");
            return View();
        }

        // POST: ArtOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Order_Number,Customer_Id,Set_Up_Charges,Deposit,Order_Date,Due_Date,Approval_Date,Scheduled_Print_Date,Delivery_Date,Apparel_Item_Id,Art_Date,Art_Film_Date")] ArtOrder artOrder)
        {
            if (ModelState.IsValid)
            {
                db.ArtOrders.Add(artOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Apparel_Item_Id = new SelectList(db.ApparelItems, "Apparel_Item_Id", "Base_Color", artOrder.Apparel_Item_Id);
            ViewBag.Customer_Id = new SelectList(db.Customers, "Customer_Id", "Customer_Name", artOrder.Customer_Id);
            return View(artOrder);
        }

        // GET: ArtOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArtOrder artOrder = db.ArtOrders.Find(id);
            if (artOrder == null)
            {
                return HttpNotFound();
            }
            ViewBag.Apparel_Item_Id = new SelectList(db.ApparelItems, "Apparel_Item_Id", "Apparel_Item", artOrder.Apparel_Item_Id);
            ViewBag.Customer_Id = new SelectList(db.Customers, "Customer_Id", "Customer_Name", artOrder.Customer_Id);
            ViewBag.Employee_Id = db.Employees.ToList();
            ViewBag.Vendor_Id = db.Vendors.Select(x => x.Vendor_Name).Distinct();
            ApparelItem apparel = db.ApparelItems.Find(artOrder.Apparel_Item_Id);
            artOrder.ApparelItem = apparel;
            List<ArtLocation> locations = db.ArtLocations.Where(x => x.Apparel_Item_Id == artOrder.Apparel_Item_Id).ToList();
            artOrder.ApparelItem.ArtLocations = locations;
            artOrder.ApparelItem.locations = locations;
            return View(artOrder);
        }

        // POST: ArtOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ArtOrder artOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(artOrder).State = EntityState.Modified;
                ArtOrder order = db.ArtOrders.Find(artOrder.Order_Number);
                order.Order_Date = artOrder.Order_Date;
                string task = "[" + artOrder.ApparelItem.Apparel_Item_Id + "] " + artOrder.ApparelItem.Apparel_Item + " " + artOrder.ApparelItem.Base_Color;
                List<Project> projects = new List<Project>();
                foreach (var location in artOrder.ApparelItem.locations)
                {
                    ArtLocation artLoc = db.ArtLocations.Find(location.Art_Location_Id);
                    artLoc.Employee_Id = location.Employee_Id;
                    artLoc.Completion_Date = location.Completion_Date;
                    Project project = new Project();
                    project.Apparel_Item_Id = location.Apparel_Item_Id;
                    project.Project_Date = DateTime.Now;
                    project.Task_Details = task + ": " + location.Art_Location + "-> " + location.Description;
                    projects.Add(project);
                }
                db.Projects.AddRange(projects);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Apparel_Item_Id = new SelectList(db.ApparelItems, "Apparel_Item_Id", "Base_Color", artOrder.Apparel_Item_Id);
            ViewBag.Customer_Id = new SelectList(db.Customers, "Customer_Id", "Customer_Name", artOrder.Customer_Id);
            return View(artOrder);
        }

        // GET: ArtOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArtOrder artOrder = db.ArtOrders.Find(id);
            if (artOrder == null)
            {
                return HttpNotFound();
            }
            return View(artOrder);
        }

        // POST: ArtOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ArtOrder artOrder = db.ArtOrders.Find(id);
            db.ArtOrders.Remove(artOrder);
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
