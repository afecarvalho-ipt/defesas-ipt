export function generateSlotsTable() {
  let i = 0;

  let duration = Number($("#_SlotDuration").val());
  let start = $("#_DayStart").val();
  let end = $("#_DayEnd").val();

  let slotsTable = $("#TimeTableSlots");
  slotsTable.attr("hidden", "hidden");

  let slotsRows = slotsTable.find("tbody");
  slotsRows.empty();

  for (let slot of generateTimetable(duration, start, end)) {
    let template = $(`
        <tr>
            <td>
                <input type="hidden" name="Slots[${i}].StartsAt" value="${slot.start}" />
                <input type="hidden" name="Slots[${i}].EndsAt" value="${slot.end}" />

                ${slot.start}&nbsp;-&nbsp;${slot.end}
            </td>
            <td>
                <input type="text" name="Slots[${i}].Description" value="" maxlength="512" class="form-control" />
            </td>
            <td>
                <input type="checkbox" value="true" checked name="Slots[${i}].Available" />
            </td>
        </tr>
    `);

    template.appendTo(slotsRows);

    i += 1;
  }

  slotsTable.removeAttr("hidden");
}

function dateToIsoDateString(d) {
  let year = d.getFullYear();
  let month = String(d.getMonth() + 1).padStart(2, "0");
  let day = String(d.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function* generateTimetable(duration, start, end) {
  let whenAsString = dateToIsoDateString(new Date());

  let startAsMs = new Date(`${whenAsString} ${start}`).getTime();
  let endAsMs = new Date(`${whenAsString} ${end}`).getTime();

  let durationInMs = duration * 60 * 1000;

  let currentMs = startAsMs;

  while (currentMs < endAsMs) {
    let item = {
      start: dateToHourMinute(new Date(currentMs)),
      end: dateToHourMinute(new Date(currentMs + durationInMs)),
    };

    currentMs += durationInMs;
    yield item;
  }
}

function dateToHourMinute(d) {
  let hours = String(d.getHours()).padStart(2, "0");
  let minutes = String(d.getMinutes()).padStart(2, "0");

  return `${hours}:${minutes}`;
}
