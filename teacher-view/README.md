# Teacher view

An offline companion page for teachers. It shows a class's progress through the
Potential and Kinetic Energy module at a glance, from the progress files exported
by the headsets. No server, no internet, no install.

## How to use
1. Copy `teacher.html` (and, for a demo, the `sample/` folder) onto any laptop or
   phone.
2. Double click `teacher.html`. It opens in any web browser, fully offline.
3. Choose the class export file, or drag it onto the page. You can load one file
   that holds the whole class, or several single learner files at once.
4. To try it now, load `sample/class-sample.json` (a made-up 12 seat class).

## What you see
- **Class overview:** one row per headset seat, with a mastery percent for each of
  the five objectives, an overall figure, how many times a different peer guide
  re-explained a question, and a status. Seats with an objective below 50 percent
  are highlighted and marked "Needs review", and a banner counts them.
- **Learner detail:** select any row to see that seat's objectives (with questions
  answered and attempts), guide switches, and the hands-on station results (throw
  speeds and energies, hill release heights and outcomes, and how far short of the
  nose the swing returned).

## Printing
Use the browser's Print. The class overview prints for paper record keeping; the
selected learner's detail prints on a following page. Turn on "Background
graphics" in the print dialog if you want the colour bands to appear on paper. The
mastery percent and the "Needs review" wording print either way, so the record is
readable in black and white.

## Privacy
The page reads only the file you choose, on your own device, and sends nothing
anywhere. The export files carry no personal data: seats are identified by a
device-local slot id, never by a learner's name or any personal detail.
