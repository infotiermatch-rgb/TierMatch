const animalValueLabels: Record<string, string> = {
  Dog: "Hund",
  Cat: "Katze",
  Rabbit: "Kaninchen",
  GuineaPig: "Meerschweinchen",
  Bird: "Vogel",
  Reptile: "Reptil",
  Fish: "Fisch",
  Other: "Sonstiges",

  Male: "Männlich",
  Female: "Weiblich",
  Unknown: "Unbekannt",

  Small: "Klein",
  Medium: "Mittel",
  Large: "Groß",

  Available: "Vermittelbar",
  Reserved: "Reserviert",
  Adopted: "Vermittelt",
  NotAvailable: "Nicht vermittelbar",
};

export function formatAnimalValue(
  value: string | null | undefined,
  fallback = "Keine Angabe",
): string {
  if (!value?.trim()) {
    return fallback;
  }

  if (animalValueLabels[value]) {
    return animalValueLabels[value];
  }

  return value
    .replace(/([a-zäöü])([A-ZÄÖÜ])/g, "$1 $2")
    .replaceAll("_", " ");
}

export function formatAnimalBirthDate(
  birthDate: string | null,
): string {
  if (!birthDate) {
    return "Unbekannt";
  }

  const parsedDate = new Date(
    `${birthDate}T00:00:00`,
  );

  if (Number.isNaN(parsedDate.getTime())) {
    return "Unbekannt";
  }

  return new Intl.DateTimeFormat("de-DE", {
    dateStyle: "long",
  }).format(parsedDate);
}

export function formatAnimalAge(
  birthDate: string | null,
): string {
  if (!birthDate) {
    return "Alter unbekannt";
  }

  const parsedDate = new Date(
    `${birthDate}T00:00:00`,
  );

  if (Number.isNaN(parsedDate.getTime())) {
    return "Alter unbekannt";
  }

  const today = new Date();

  let years =
    today.getFullYear() -
    parsedDate.getFullYear();

  const monthDifference =
    today.getMonth() -
    parsedDate.getMonth();

  if (
    monthDifference < 0 ||
    (monthDifference === 0 &&
      today.getDate() < parsedDate.getDate())
  ) {
    years -= 1;
  }

  if (years > 0) {
    return years === 1
      ? "1 Jahr"
      : `${years} Jahre`;
  }

  let months =
    (today.getFullYear() -
      parsedDate.getFullYear()) *
      12 +
    today.getMonth() -
    parsedDate.getMonth();

  if (today.getDate() < parsedDate.getDate()) {
    months -= 1;
  }

  if (months <= 0) {
    return "Unter 1 Monat";
  }

  return months === 1
    ? "1 Monat"
    : `${months} Monate`;
}