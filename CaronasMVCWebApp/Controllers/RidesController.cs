﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaronasMVCWebApp.Models;
using CaronasMVCWebApp.Services;
using CaronasMVCWebApp.Models.ViewModels;
using System;
using CaronasMVCWebApp.Models.Enums;

namespace CaronasMVCWebApp.Controllers
{
    public class RidesController : Controller
    {
        private readonly caronas_app_dbContext _context;
        private readonly MemberService _memberService;
        private readonly DestinyService _destinyService;
        private readonly RideService _rideService;

        public RidesController(caronas_app_dbContext context,
                               MemberService memberService,
                               DestinyService destinyService,
                               RideService rideService)
        {
            _context = context;
            _memberService = memberService;
            _destinyService = destinyService;
            _rideService = rideService;
        }

        // GET: Rides
        public async Task<IActionResult> Index()
        {
            var caronas_app_dbContext = await _context.Ride.Include(r => r.Destiny).Include(r => r.Driver).OrderByDescending(r=>r.Date).ToListAsync();
            var results = caronas_app_dbContext.Select(r => r.Id).Distinct().ToList();

            List<Ride> rides = new List<Ride>();
            foreach (int id in results)
            {
                var ride = await _context.Ride.Where(r => r.Id == id).FirstOrDefaultAsync();
                ride.Destiny = await _destinyService.FindByIdAsync(ride.DestinyId);
                ride.Driver = await _memberService.FindByIdAsync(ride.DriverId);
                var passengers = await _rideService.FindPassengersByRideId(ride.Id);
                ride.PassengerId = passengers.Count();
                rides.Add(ride);
                
            }
            return View(rides);
        }

        // GET: Rides/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Ride ride = await _context.Ride.Where(r => r.Id == id).FirstOrDefaultAsync();
            if (ride == null)
            {
                return NotFound();
            }

            RideFormViewModel viewModel = new RideFormViewModel()
            {
                Ride = await _context.Ride.Where(r => r.Id == id).FirstOrDefaultAsync()
            };
            viewModel = await _rideService.StartRideViewModel(viewModel);

            //int nextId = obj.Id == 0 ? await FindNextIdAsync() : obj.Id;
            var roundTripValue = viewModel.Ride.RoundTrip.Equals(RoundTrip.RoundTrip) ? "Ida e volta" : "Apenas ida/volta";
            ViewData["RoundTripValue"] = roundTripValue;
            ViewBag.Title = "Detalhes da carona";
            ViewBag.Action = "Details";
            return View("Create", viewModel);
        }

        // GET: Rides/Create
        public async Task<IActionResult> Create()
        {
            DateTime date = await _context.Ride.AnyAsync() ? await _context.Ride.MaxAsync(r => r.Date) : DateTime.Now;
            RideFormViewModel viewModel = new RideFormViewModel(date.AddDays(1.0));
            viewModel = await _rideService.StartRideViewModel(viewModel);
            ViewBag.Title = "Nova carona";
            ViewBag.Action = "Create";
            return View(viewModel);
        }

        // POST: Rides/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RideFormViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var Passengers = viewModel.Passengers.Where(x => x.IsChecked).Select(x => x.ID).ToList();
                //Validate if any passenger was selected
                if (Passengers.Count > 0)
                {
                    //Validate if the Driver is also a Passenger
                    if (!Passengers.Contains(viewModel.Ride.DriverId))
                    {
                        await _rideService.InsertAsync(viewModel.Ride, Passengers);
                        TempData["SuccessMessage"] = "Carona cadastrada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "O motorista não pode ser um dos passageiros";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Selecione pelo menos 1 passageiros";
                }
            }
            viewModel = await _rideService.StartRideViewModel(viewModel);
            ViewBag.Title = "Nova carona";
            ViewBag.Action = "Create";
            return View(viewModel);
        }

        // GET: Rides/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {


            if (id == null)
            {
                return NotFound();
            }

            Ride ride = await _context.Ride.Where(r => r.Id == id).FirstOrDefaultAsync();
            if (ride == null)
            {
                return NotFound();
            }

            RideFormViewModel viewModel = new RideFormViewModel()
            {
                Ride = await _context.Ride.Where(r => r.Id == id).FirstOrDefaultAsync()
            };
            viewModel = await _rideService.StartRideViewModel(viewModel);

            ViewBag.Title = "Editar carona";
            ViewBag.Action = "Edit";
            return View("Create", viewModel);
        }

        // POST: Rides/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RideFormViewModel viewModel)
        {
            if (id != viewModel.Ride.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var Passengers = viewModel.Passengers.Where(x => x.IsChecked).Select(x => x.ID).ToList();
                    //Validate if any passenger was selected
                    if (Passengers.Count > 0)
                    {
                        //Validate if the Driver is also a Passenger
                        if (!Passengers.Contains(viewModel.Ride.DriverId))
                        {
                            await _rideService.UpdateAsync(viewModel.Ride, Passengers);
                            TempData["SuccessMessage"] = "Carona alterada com sucesso!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "O motorista não pode ser um dos passageiros";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Selecione pelo menos 1 passageiros";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RideExists(viewModel.Ride.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            viewModel = await _rideService.StartRideViewModel(viewModel);
            ViewBag.Title = "Editar carona";
            ViewBag.Action = "Edit";
            return View("Create", viewModel);
        }

        // GET: Rides/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ride = await _context.Ride.FirstOrDefaultAsync(r => r.Id == id);
            if (ride == null)
            {
                return NotFound();
            }

            RideFormViewModel viewModel = new RideFormViewModel()
            {
                Ride = ride
            };
            viewModel = await _rideService.StartRideViewModel(viewModel);
            ViewBag.Title = "Editar carona";
            ViewBag.Action = "Delete";
            return View("Create", viewModel);
        }

        // POST: Rides/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rides = await _context.Ride.Where(r => r.Id == id).ToListAsync();
            foreach (var ride in rides)
            {

                _context.Ride.Remove(ride);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Carona excluída com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        private bool RideExists(int id)
        {
            return _context.Ride.Any(e => e.Id == id);
        }
    }
}
