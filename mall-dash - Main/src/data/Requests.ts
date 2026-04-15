import type { Request } from '@/interfaces/Request.interface';

const mockRequests: Request[] = [
  {
    id: 'REQ-001',
    tenantId: 'TEN-001',
    tenantName: 'Fashion Boutique',
    title: 'AC Not Cooling',
    description:
      'The air conditioning unit in the main showroom is not cooling properly. It has been running continuously for the past two days but the temperature inside remains uncomfortably high, reaching around 28°C during peak hours. Customers have been complaining and some have left without making purchases. We have tried adjusting the thermostat and cleaning the visible filters but the issue persists. This is affecting our business significantly and we request urgent attention.',
    status: 'Pending',
    images: [
      'https://images.unsplash.com/photo-1517646287270-a5a9ca602e5c?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
      'https://images.unsplash.com/photo-1585771724684-38269d6639fd?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
    ],
    createdAt: '2023-10-25T10:00:00Z',
    updatedAt: '2023-10-25T10:00:00Z',
  },
  {
    id: 'REQ-002',
    tenantId: 'TEN-002',
    tenantName: 'Tech Gadgets Store',
    title: 'Leaking Pipe in Storage',
    description:
      'There is a small but persistent leak in the water pipe running through our back storage room. We first noticed it yesterday morning when we found a puddle near our electronics inventory boxes. We have placed buckets to catch the dripping water as a temporary measure, but we are concerned about potential water damage to our stock and the possibility of mold growth. The leak appears to be coming from a joint in the pipe near the ceiling. Please send a plumber to inspect and repair as soon as possible.',
    status: 'In Progress',
    images: [],
    createdAt: '2023-10-24T14:30:00Z',
    updatedAt: '2023-10-25T09:15:00Z',
  },
  {
    id: 'REQ-003',
    tenantId: 'TEN-003',
    tenantName: 'Coffee Corner',
    title: 'Broken Door Handle',
    description:
      'The main entrance door handle is loose and about to fall off completely. It has been getting progressively worse over the past week and now wobbles significantly when customers try to open the door. This is both a safety concern and creates a poor first impression for our café. The screws appear to be stripped and the handle mechanism may need complete replacement. We have had to prop the door open during business hours as a temporary solution.',
    status: 'Resolved',
    images: [
      'https://images.unsplash.com/photo-1517646287270-a5a9ca602e5c?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
    ],
    createdAt: '2023-10-20T08:00:00Z',
    updatedAt: '2023-10-22T16:45:00Z',
  },
  {
    id: 'REQ-004',
    tenantId: 'TEN-001',
    tenantName: 'Fashion Boutique',
    title: 'Request for Extra Cleaning',
    description:
      'We had a significant spill in the fitting room area this afternoon when a customer accidentally knocked over a large coffee drink. The liquid has seeped into the carpet and left a noticeable stain that our staff was unable to fully remove with basic cleaning supplies. There is also a lingering odor that is quite unpleasant for customers using the fitting rooms. We kindly request a professional deep cleaning service at your earliest convenience to restore the area to its proper condition.',
    status: 'Pending',
    images: [],
    createdAt: '2023-10-26T11:20:00Z',
    updatedAt: '2023-10-26T11:20:00Z',
  },
  {
    id: 'REQ-005',
    tenantId: 'TEN-004',
    tenantName: 'Book Worms',
    title: 'Power Outage in Section B',
    description:
      "The lights in the children's book section have been flickering intermittently throughout the day and have now gone out completely in the back corner of the area. This section is popular with families and the poor lighting makes it difficult for customers to browse the shelves safely. We have checked our circuit breaker and it appears to be functioning normally, so the issue may be with the building's electrical supply to this area. Additionally, two of our point-of-sale terminals in this zone have also lost power.",
    status: 'Pending',
    images: [],
    createdAt: '2023-10-26T09:00:00Z',
    updatedAt: '2023-10-26T09:00:00Z',
  },
];

export const fetchMockRequests = async (
  page: number = 1,
  limit: number = 10,
  search: string = '',
  status: string = 'All'
): Promise<{ data: Request[]; total: number }> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      let filtered = [...mockRequests];

      if (search) {
        const lowerSearch = search.toLowerCase();
        filtered = filtered.filter(
          (r) =>
            r.title.toLowerCase().includes(lowerSearch) ||
            r.tenantName.toLowerCase().includes(lowerSearch) ||
            r.id.toLowerCase().includes(lowerSearch)
        );
      }

      if (status && status !== 'All') {
        filtered = filtered.filter((r) => r.status === status);
      }

      const start = (page - 1) * limit;
      const end = start + limit;
      const data = filtered.slice(start, end);

      resolve({
        data,
        total: filtered.length,
      });
    }, 800); // Simulate network delay
  });
};

export const fetchMockRequestById = async (
  id: string
): Promise<Request | null> => {
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      const request = mockRequests.find((r) => r.id === id);
      if (request) {
        resolve(request);
      } else {
        reject(new Error('Request not found'));
      }
    }, 500);
  });
};

