export interface CurrentUser {
  id: string;
  phoneNumber: string;
  fullName: string;
  role: number;
  trustScore: number;
  avatarUrl?: string | null;
}

export interface UpdateCurrentUser {
  fullName: string;
}
