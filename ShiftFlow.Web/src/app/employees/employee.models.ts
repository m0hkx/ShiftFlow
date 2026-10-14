export interface Employee {
  id: string;
  name: string;
  email: string;
  position: string;
  team: string | null;
  active: boolean;
}

export interface CreateEmployee {
  name: string;
  email: string;
  position: string;
  team?: string | null;
  active: boolean;
}
