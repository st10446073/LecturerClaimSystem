using Microsoft.AspNetCore.Mvc;
using LecturerClaimSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace LecturerClaimSystem.Controllers
{
    public class ClaimsController : Controller
    {
        // In-memory storage for demonstration
        private static List<Claim> claims = new List<Claim>();

        // GET: Claims/Index
        // GET: Claims/Index
        public IActionResult Index()
        {
            ViewBag.IsVerifier = User?.Identity != null && User.Identity.IsAuthenticated &&
                                (User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager"));
            return View(claims);
        }

        // GET: Claims/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Claim claim, IFormFile? document)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (document != null && document.Length > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                    var ext = Path.GetExtension(document.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("DocumentPath", "File type not allowed.");
                        return View(claim);
                    }

                    if (document.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("DocumentPath", "File too large (max 5MB).");
                        return View(claim);
                    }

                    var uploads = Path.Combine(Path.GetTempPath(), "LecturerUploads");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var filePath = Path.Combine(uploads, document.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        document.CopyTo(stream);
                    }

                    claim.DocumentPath = "/uploads/" + document.FileName;
                }

                // Assign ID, status, and date
                claim.Id = claims.Count + 1;
                claim.Status = "Pending";
                claim.DateSubmitted = System.DateTime.Now;

                claims.Add(claim);
                return RedirectToAction("Index");
            }

            return View(claim);
        }

        // GET: Claims/Verify
        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult Verify()
        {
            // Only show pending claims for verification
            var pendingClaims = claims.Where(c => c.Status == "Pending").ToList();
            return View(pendingClaims);
        }

        // Approve a claim via AJAX
        [HttpPost]
        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult ApproveAjax(int id)
        {
            var claim = claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Approved";
                return Json(new { success = true, status = claim.Status });
            }
            return Json(new { success = false });
        }

        // Reject a claim via AJAX
        [HttpPost]
        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult RejectAjax(int id)
        {
            var claim = claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                return Json(new { success = true, status = claim.Status });
            }
            return Json(new { success = false });
        }

        // GET: Claims/GetClaims (returns all claims as JSON)
        [HttpGet]
        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult GetClaims()
        {
            return Json(claims);
        }
    }
}
