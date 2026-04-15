export interface Request {
  id: string;
  tenantId: string;
  tenantName: string;
  title: string;
  description: string;
  status: 'Pending' | 'In Progress' | 'Resolved' | 'Rejected';
  images: string[];
  createdAt: string;
  updatedAt: string;
}

export interface RequestsState {
  requests: Request[];
  selectedRequest: Request | null;
  loading: boolean;
  error: string | null;
  total: number;
  page: number;
  limit: number;
}

