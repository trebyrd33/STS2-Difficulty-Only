using System.IO;
namespace MultiplayerExpanded.Extensions;
public static class StringExtensions {
 private static string Stem(string path) => Path.GetFileNameWithoutExtension(path);
 public static string ImagePath(this string path) => $"res://STS2DifficultyOnly/images/{Path.ChangeExtension(path, ".tres")}";
 public static string EventImagePath(this string path) => $"res://STS2DifficultyOnly/images/events/{Stem(path)}.tres";
 public static string RelicImagePath(this string path) => $"res://STS2DifficultyOnly/images/relics/{Stem(path)}.tres";
 public static string BigRelicImagePath(this string path) => RelicImagePath(path);
 public static string CardImagePath(this string path) => $"res://STS2DifficultyOnly/images/card_portraits/{Stem(path)}.tres";
 public static string BigCardImagePath(this string path) => CardImagePath(path);
 public static string PowerImagePath(this string path) => $"res://STS2DifficultyOnly/images/powers/{Stem(path)}.tres";
 public static string BigPowerImagePath(this string path) => PowerImagePath(path);
}
