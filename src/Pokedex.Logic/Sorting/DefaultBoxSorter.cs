using Pokedex.Models.LivingDex;

namespace Pokedex.Logic.Sorting
{
    public class DefaultBoxSorter : IBoxSorter
    {
        public List<LivingDexBox> Sort(List<LivingEntry> entries)
        {

            // All of these forms should go to the end
            var pikachuCaps = entries.Where(e => e.IsPikachuCap == true).ToList();

            // Alola, Galar, Hisui, Paldean
            var regionalForms = entries
                .Where(e => e.IsRegionalForm == true)
                .GroupBy(e => new 
                {
                    e.Form.Region,
                    e.Form.Generation
                })
                .OrderBy(e => e.Key.Generation)
                .ThenBy(e => e.Key.Region)
                .Select(group => group.Select(list => list).ToList())
                .ToArray();

            var specialForms = new List<List<LivingEntry>>();
            specialForms.Add(pikachuCaps);
            specialForms.AddRange(regionalForms);

            var regularForms = entries;
            foreach (var form in specialForms)
                regularForms = regularForms.Except(form).ToList();


            // Apply sort
            var results = new List<LivingDexBox>();
            results = results.Union(GetBoxes(regularForms)).ToList();

            foreach (var form in specialForms)
                results = results.Union(GetBoxes(form)).ToList();

            var count = 1;
            foreach(var box in results)
            {
                box.BoxNumber = count;
                count++;
            }

            return results;
        }

        private List<LivingDexBox> GetBoxes(List<LivingEntry> entries)
        {
            var results = new List<LivingDexBox>();

            entries = entries
                .OrderBy(e => e.Form.DexNum)
                .ThenBy(e => e.Form.IsFemaleForm)
                .ToList();

            var page = 1;
            var pageSize = 30;

            while (true)
            {
                // Get next page
                var toSkip = (page - 1) * pageSize;
                var toAdd = entries.Skip(toSkip).Take(pageSize).ToList();
                if (toAdd.Any() == false)
                    break;

                // Populate the box
                var box = new LivingDexBox();
                box.SlotCount = pageSize;

                foreach(var add in toAdd)
                    box.Slots.Add(add);

                // Fill empty slots with dummy data
                while(box.IsFull == false)
                {
                    var empty = new LivingEntry(null);
                    box.Slots.Add(empty);
                }

                results.Add(box);

                page++;
            }


            return results;
        }


    }
}
