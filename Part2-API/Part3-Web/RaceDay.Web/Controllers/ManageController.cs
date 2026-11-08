using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Diagnostics.Metrics;

namespace RaceDay.Web.Controllers;


[SessionAuthorize(Roles.Organiser)]
public class ManageController : AppController
{
    public ManageController(ApiClient api) : base(api) { }

    // ---------- Dashboard ----------
    public async Task<IActionResult> Dashboard()
    {
        var result = await Api.GetAsync<OrganiserEventList>("api/events/mine");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;
        if (!result.Success) Flash(result.Error ?? "We could not load your dashboard.", false);

        return View(result.Data?.Events ?? new List<OrganiserEvent>());
    }

    // ---------- Events: list, create, edit, delete ----------
    public async Task<IActionResult> Events()
    {
        var result = await Api.GetAsync<OrganiserEventList>("api/events/mine");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;
        if (!result.Success) Flash(result.Error ?? "We could not load your events.", false);

        return View(result.Data?.Events ?? new List<OrganiserEvent>());
    }

    [HttpGet]
    public IActionResult Create() => View(new EventForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventForm form)
    {
        if (!ModelState.IsValid) return View(form);

        var result = await Api.PostAsync<CreateEventResult>("api/events", new
        {
            form.Name,
            form.Description,
            form.Date,
            form.Location,
            form.Distance,
            form.Capacity,
            form.EventType
        });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not create the event.");
            return View(form);
        }

        await UploadBannerIfProvided(result.Data.EventId, form.Banner);
        Flash("Event created. Now add some categories to it.");
        return RedirectToAction(nameof(Categories), new { id = result.Data.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await Api.GetAsync<EventDetail>($"api/events/{id}");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (!result.Success || result.Data is null)
        {
            Flash(result.Error ?? "We could not find that event.", false);
            return RedirectToAction(nameof(Events));
        }

        var e = result.Data;
        return View(new EventForm
        {
            Id = e.EventId,
            Name = e.Name,
            Description = e.Description,
            Date = e.Date,
            Location = e.Location,
            Distance = e.Distance,
            Capacity = e.Capacity,
            EventType = e.EventType,
            Status = e.Status,
            BannerImage = e.BannerImage
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EventForm form)
    {
        form.Id = id;
        if (!ModelState.IsValid) return View(form);

        var result = await Api.PutAsync<object>($"api/events/{id}", new
        {
            form.Name,
            form.Description,
            form.Date,
            form.Location,
            form.Distance,
            form.Capacity,
            form.EventType,
            form.Status
        });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not save the event.");
            return View(form);
        }

        await UploadBannerIfProvided(id, form.Banner);
        Flash("Event updated.");
        return RedirectToAction(nameof(Events));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Api.DeleteAsync($"api/events/{id}");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (result.Success) Flash("Event deleted.");
        else Flash(result.Error ?? "We could not delete that event.", false);

        return RedirectToAction(nameof(Events));
    }

    // ---------- Categories ----------
    public async Task<IActionResult> Categories(Guid id)
    {
        var eventResult = await Api.GetAsync<EventDetail>($"api/events/{id}");
        if (ExpiredSession(eventResult.StatusCode) is { } expired) return expired;

        if (!eventResult.Success || eventResult.Data is null)
        {
            Flash(eventResult.Error ?? "We could not find that event.", false);
            return RedirectToAction(nameof(Events));
        }

        return View(await BuildCategoriesModel(eventResult.Data, new CategoryForm()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(Guid id, Guid categoryId)
    {
        var result = await Api.PostAsync<EventCategoryItem>($"api/events/{id}/categories", new { categoryId });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (result.Success) Flash("Category added to the event.");
        else Flash(result.Error ?? "We could not add that category.", false);

        return RedirectToAction(nameof(Categories), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Guid id, CategoryForm form)
    {
        if (form.MinAge.HasValue && form.MaxAge.HasValue && form.MinAge > form.MaxAge)
            ModelState.AddModelError(nameof(form.MaxAge), "Maximum age cannot be lower than minimum age.");

        if (!ModelState.IsValid)
        {
            var eventResult = await Api.GetAsync<EventDetail>($"api/events/{id}");
            if (!eventResult.Success || eventResult.Data is null) return RedirectToAction(nameof(Events));
            return View("Categories", await BuildCategoriesModel(eventResult.Data, form));
        }

        var created = await Api.PostAsync<CreateCategoryResult>("api/categories",
            new { form.Name, form.Description, form.MinAge, form.MaxAge });
        if (ExpiredSession(created.StatusCode) is { } expired) return expired;

        if (!created.Success || created.Data is null)
        {
            Flash(created.Error ?? "We could not create that category.", false);
            return RedirectToAction(nameof(Categories), new { id });
        }

        var added = await Api.PostAsync<EventCategoryItem>($"api/events/{id}/categories",
            new { categoryId = created.Data.CategoryId });

        if (added.Success) Flash("Category created and added to the event.");
        else Flash(added.Error ?? "The category was created but could not be added to the event.", false);

        return RedirectToAction(nameof(Categories), new { id });
    }

    // ---------- Enrolments ----------
    public async Task<IActionResult> Enrolments(Guid id)
    {
        var eventResult = await Api.GetAsync<EventDetail>($"api/events/{id}");
        if (ExpiredSession(eventResult.StatusCode) is { } expired) return expired;

        var list = await Api.GetAsync<EventEnrollmentList>($"api/events/{id}/enrollments");
        if (!eventResult.Success || eventResult.Data is null || !list.Success)
        {
            Flash(list.Error ?? eventResult.Error ?? "We could not load the enrolments.", false);
            return RedirectToAction(nameof(Events));
        }

        return View(new ManageEnrolmentsViewModel
        {
            Event = eventResult.Data,
            Enrollments = list.Data?.Enrollments ?? new List<EventEnrollment>()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(Guid id, Guid enrollmentId, string status)
    {
        var result = await Api.PutAsync<object>($"api/enrollments/{enrollmentId}/status", new { status });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (result.Success) Flash($"Enrolment marked as {status}.");
        else Flash(result.Error ?? "We could not change that enrolment.", false);

        return RedirectToAction(nameof(Enrolments), new { id });
    }

    
    public async Task<IActionResult> Results(Guid id)
    {
        var eventResult = await Api.GetAsync<EventDetail>($"api/events/{id}");
        if (ExpiredSession(eventResult.StatusCode) is { } expired) return expired;

        var enrolments = await Api.GetAsync<EventEnrollmentList>($"api/events/{id}/enrollments");
        var results = await Api.GetAsync<EventResultList>($"api/events/{id}/results");

        if (!eventResult.Success || eventResult.Data is null || !enrolments.Success || !results.Success)
        {
            Flash(results.Error ?? enrolments.Error ?? eventResult.Error ?? "We could not load the results page.", false);
            return RedirectToAction(nameof(Events));
        }

        var recorded = results.Data?.Results ?? new List<EventResult>();
        var done = recorded.Select(r => r.EnrollmentId).ToHashSet();
        var awaiting = (enrolments.Data?.Enrollments ?? new List<EventEnrollment>())
            .Where(e => e.Status != "Cancelled" && !done.Contains(e.EnrollmentId))
            .ToList();

        return View(new ManageResultsViewModel { Event = eventResult.Data, Awaiting = awaiting, Results = recorded });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(Guid id, ResultForm form)
    {
        if (!ModelState.IsValid)
        {
            var problems = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            Flash(string.Join(" ", problems), false);
            return RedirectToAction(nameof(Results), new { id });
        }

        var result = await Api.PostAsync<object>("api/results", new
        {
            form.EnrollmentId,
            form.FinishTime,
            form.Position,
            form.ChipNumber
        });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (result.Success) Flash("Result saved.");
        else Flash(result.Error ?? "We could not save that result.", false);

        return RedirectToAction(nameof(Results), new { id });
    }

   
    private async Task UploadBannerIfProvided(Guid eventId, IFormFile? banner)
    {
        if (banner is not { Length: > 0 }) return;

        var upload = await Api.UploadImageAsync($"api/events/{eventId}/banner", banner);
        if (!upload.Success)
            Flash("The event was saved, but the banner could not be uploaded: " + upload.Error, false);
    }

    private async Task<ManageCategoriesViewModel> BuildCategoriesModel(EventDetail ev, CategoryForm form)
    {
        var all = await Api.GetAsync<CategoryList>("api/categories");
        var already = ev.Categories.Select(c => c.CategoryId).ToHashSet();

        return new ManageCategoriesViewModel
        {
            Event = ev,
            NewCategory = form,
            Available = (all.Data?.Categories ?? new List<CategoryItem>())
                .Where(c => !already.Contains(c.CategoryId)).ToList()
        };
    }
}
