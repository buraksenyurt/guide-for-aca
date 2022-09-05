using BalladMngr.Domain.Entities;
using BalladMngr.Domain.Enums;

namespace BalladMngr.Data.Contexts
{
    /*
     * Seed operasyonu için kullanılan statik sınıfımız.
     * Eğer hiç şarkı yoksa birkaç tane ekleyecek.
     */
    public static class BalladMngrDbContextSeed
    {
        public static async Task SeedDataAsync(BalladMngrDbContext context)
        {
            if (!context.Songs.Any())
            {
                await context.Songs.AddAsync(new Song
                {
                    Title = "The Wall",
                    Lyrics = @"We don't need no education
                                We don't need no thought control
                                No dark sarcasm in the classroom
                                Teacher, leave them kids alone",
                    Language = Language.English,
                    Status = Status.Draft
                });
                await context.Songs.AddAsync(new Song
                {
                    Title = "Mazeretim Var Asabiyim Ben",
                    Lyrics = @"Gülmüyor yüzüm hayat zor oldu
                                Güller susuz kurudu soldu
                                Tövbe ettim gene bozuldu
                                Yüreğim yanar
                                Mazeretim var; asabiyim ben
                                Mazeretim var; asabiyim ben",
                    Language = Language.Turkish,
                    Status = Status.Completed
                });
                await context.Songs.AddAsync(new Song
                {
                    Title = "Dönence",
                    Lyrics = @"Simsiyah gecenin koynundayım yapayalnız
                                Uzaklarda bir yerlerde güneşler doğuyor
                                Biliyorum
                                Dönence",
                    Language = Language.Turkish,
                    Status = Status.Draft
                });
                await context.SaveChangesAsync();
            }
        }
    }
}