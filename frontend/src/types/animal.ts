export type AnimalDto = {
  id: string;
  name: string;
  breed: string;
  species: string;
  gender: string;
  size: string;
  birthDate: string | null;
  description: string;
  isVaccinated: boolean;
  isCastrated: boolean;
  shelterId: string | null;
  shelterName: string | null;
  status: string;
};

export type AnimalImageDto = {
  id: string;
  fileName: string;
  filePath: string;
  contentType: string;
  fileSize: number;
  isPrimary: boolean;
  sortOrder: number;
  url: string;
};