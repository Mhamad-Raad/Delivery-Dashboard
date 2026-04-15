import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Package,
  ArrowLeft,
  Save,
  Upload,
  X,
  ImageIcon,
  Tag,
  DollarSign,
  FolderOpen,
  Store,
  PackageCheck,
  Scale,
} from 'lucide-react';

import { createProduct } from '@/data/Products';
import { fetchCategories, type CategoryDTO } from '@/data/Categories';

export default function CreateProduct() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    categoryId: '',
    name: '',
    description: '',
    price: '',
    discountPrice: '',
    inStock: true,
    isWeightable: false,
    photo: null as File | null,
    imageUrl: '',
  });

  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState<CategoryDTO[]>([]);

  useEffect(() => {
    const run = async () => {
      const res = await fetchCategories();
      if ((res as any)?.error) {
        toast.error('Failed to load categories');
      } else {
        setCategories(res as CategoryDTO[]);
      }
    };
    run();
  }, []);

  const handleInputChange = (field: string, value: any) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setFormData((prev) => ({ ...prev, photo: file }));
      const reader = new FileReader();
      reader.onloadend = () => {
        setPreviewUrl(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleRemovePhoto = () => {
    setFormData((prev) => ({ ...prev, photo: null }));
    setPreviewUrl(null);
  };

  const handleBack = () => {
    navigate('/products');
  };

  const handleCreateProduct = async () => {
    if (!formData.name.trim()) {
      toast.error('Product name is required');
      return;
    }
    if (!formData.description.trim()) {
      toast.error('Product description is required');
      return;
    }
    if (!formData.price || parseFloat(formData.price) <= 0) {
      toast.error('Please enter a valid price');
      return;
    }
    if (!formData.categoryId) {
      toast.error('Please select a category');
      return;
    }
    setLoading(true);

    const payload = {
      CategoryId: parseInt(formData.categoryId, 10),
      Name: formData.name,
      Description: formData.description,
      Price: parseFloat(formData.price),
      ...(formData.discountPrice
        ? { DiscountPrice: parseFloat(formData.discountPrice) }
        : {}),
      InStock: Boolean(formData.inStock),
      IsWeightable: Boolean(formData.isWeightable),
      ...(formData.photo
        ? { ProductImageUrl: formData.photo }
        : formData.imageUrl
        ? { ProductImageUrl: formData.imageUrl }
        : {}),
    };

    const res = await createProduct(payload);

    setLoading(false);

    if ((res as any)?.error) {
      toast.error('Failed to Create Product', {
        description:
          (res as any).error || 'An error occurred while creating the product.',
      });
    } else {
      toast.success('Product Created Successfully!', {
        description: `${formData.name} has been added to the system.`,
      });
      navigate('/products');
    }
  };

  const displayPreview = previewUrl || formData.imageUrl;

  return (
    <div className='flex flex-col gap-4'>
      {/* Header */}
      <div className='flex items-center gap-3'>
        <Button
          variant='ghost'
          size='icon'
          onClick={handleBack}
          className='size-9 shrink-0'
        >
          <ArrowLeft className='size-4' />
        </Button>
        <div className='flex items-center gap-3'>
          <div className='flex size-10 items-center justify-center rounded-lg bg-primary/10'>
            <Package className='size-5 text-primary' />
          </div>
          <div>
            <h1 className='text-xl font-semibold'>Create New Product</h1>
            <p className='text-sm text-muted-foreground'>
              Add a new product to your inventory
            </p>
          </div>
        </div>
      </div>

      {/* Main Content - 2 Column Grid */}
      <div className='grid grid-cols-1 lg:grid-cols-3 gap-6'>
        {/* Left Column - Image */}
        <Card className='lg:col-span-1'>
          <CardContent className='p-4'>
            <div className='space-y-4'>
              <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
                <ImageIcon className='size-4' />
                Product Image
              </div>

              {/* Image Preview */}
              <div className='relative aspect-square w-full overflow-hidden rounded-lg border bg-muted'>
                {displayPreview ? (
                  <>
                    <img
                      src={displayPreview}
                      alt={formData.name || 'Product'}
                      className='size-full object-cover'
                    />
                    <Button
                      type='button'
                      variant='secondary'
                      size='icon'
                      className='absolute top-2 right-2 size-8'
                      onClick={handleRemovePhoto}
                    >
                      <X className='size-4' />
                    </Button>
                  </>
                ) : (
                  <div className='flex size-full flex-col items-center justify-center gap-2 text-muted-foreground'>
                    <Package className='size-12' />
                    <span className='text-sm'>No image</span>
                  </div>
                )}
              </div>

              {/* Upload Button */}
              <Button
                type='button'
                variant='outline'
                className='w-full'
                onClick={() =>
                  document.getElementById('photo-upload')?.click()
                }
              >
                <Upload className='size-4 mr-2' />
                Upload Image
              </Button>
              <input
                id='photo-upload'
                type='file'
                accept='image/*'
                className='hidden'
                onChange={handlePhotoChange}
              />

              {/* Image URL */}
              <div className='space-y-2'>
                <Label htmlFor='imageUrl' className='text-xs'>
                  Or paste image URL
                </Label>
                <Input
                  id='imageUrl'
                  placeholder='https://...'
                  value={formData.imageUrl}
                  onChange={(e) =>
                    handleInputChange('imageUrl', e.target.value)
                  }
                  className='text-sm'
                />
              </div>

              {/* Quick Stats Preview */}
              <div className='pt-4 border-t space-y-3'>
                <div className='flex items-center justify-between text-sm'>
                  <span className='text-muted-foreground'>Status</span>
                  <Badge
                    variant={formData.inStock ? 'default' : 'secondary'}
                    className={
                      formData.inStock
                        ? 'bg-emerald-500/10 text-emerald-600 hover:bg-emerald-500/20'
                        : ''
                    }
                  >
                    {formData.inStock ? 'In Stock' : 'Out of Stock'}
                  </Badge>
                </div>
                <div className='flex items-center justify-between text-sm'>
                  <span className='text-muted-foreground'>Weightable</span>
                  <Badge
                    variant={formData.isWeightable ? 'default' : 'secondary'}
                    className={
                      formData.isWeightable
                        ? 'bg-blue-500/10 text-blue-600 hover:bg-blue-500/20'
                        : ''
                    }
                  >
                    {formData.isWeightable ? 'Yes' : 'No'}
                  </Badge>
                </div>
                <div className='flex items-center justify-between text-sm'>
                  <span className='text-muted-foreground'>Discount</span>
                  {(() => {
                    const price = parseFloat(formData.price) || 0;
                    const discountPrice = parseFloat(formData.discountPrice) || 0;
                    const hasDiscount = discountPrice > 0 && discountPrice < price;
                    return (
                      <Badge
                        className={
                          hasDiscount
                            ? 'bg-amber-500/10 text-amber-600 hover:bg-amber-500/20'
                            : 'bg-muted text-muted-foreground'
                        }
                      >
                        {hasDiscount
                          ? `${Math.round(((price - discountPrice) / price) * 100)}% OFF`
                          : 'No Discount'}
                      </Badge>
                    );
                  })()}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Right Column - Form Fields */}
        <Card className='lg:col-span-2'>
          <CardContent className='p-4 space-y-6'>
            {/* Basic Info Section */}
            <div className='space-y-4'>
              <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
                <Tag className='size-4' />
                Basic Information
              </div>

              <div className='grid gap-4'>
                {/* Product Name */}
                <div className='space-y-2'>
                  <Label htmlFor='name'>Product Name *</Label>
                  <Input
                    id='name'
                    value={formData.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                    placeholder='Enter product name'
                  />
                </div>

                {/* Description */}
                <div className='space-y-2'>
                  <Label htmlFor='description'>Description *</Label>
                  <Textarea
                    id='description'
                    value={formData.description}
                    onChange={(e) =>
                      handleInputChange('description', e.target.value)
                    }
                    placeholder='Enter product description'
                    rows={4}
                    className='resize-none'
                  />
                </div>

                {/* Category */}
                <div className='space-y-2'>
                  <Label className='flex items-center gap-2'>
                    <FolderOpen className='size-3.5' />
                    Category *
                  </Label>
                  <Select
                    value={formData.categoryId}
                    onValueChange={(value) =>
                      handleInputChange('categoryId', value)
                    }
                  >
                    <SelectTrigger className='w-full'>
                      <SelectValue placeholder='Select category' />
                    </SelectTrigger>
                    <SelectContent>
                      {categories.map((c) => (
                        <SelectItem key={c.id} value={String(c.id)}>
                          {c.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>

            {/* Pricing Section */}
            <div className='space-y-4 pt-4 border-t'>
              <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
                <DollarSign className='size-4' />
                Pricing
              </div>

              <div className='grid sm:grid-cols-2 gap-4'>
                {/* Price */}
                <div className='space-y-2'>
                  <Label htmlFor='price'>Regular Price *</Label>
                  <div className='relative'>
                    <span className='absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground'>
                      $
                    </span>
                    <Input
                      id='price'
                      type='number'
                      step='0.01'
                      min='0'
                      value={formData.price}
                      onChange={(e) =>
                        handleInputChange('price', e.target.value)
                      }
                      placeholder='0.00'
                      className='pl-7'
                    />
                  </div>
                  <p className='text-xs text-muted-foreground'>
                    The original price of your product before any discount. When a discount is active, this price will appear crossed out next to the discounted price.
                  </p>
                </div>

                {/* Discount Price */}
                <div className='space-y-2'>
                  <Label htmlFor='discountPrice'>Discount Price</Label>
                  <div className='relative'>
                    <span className='absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground'>
                      $
                    </span>
                    <Input
                      id='discountPrice'
                      type='number'
                      step='0.01'
                      min='0'
                      value={formData.discountPrice}
                      onChange={(e) =>
                        handleInputChange('discountPrice', e.target.value)
                      }
                      placeholder='0.00'
                      className='pl-7'
                    />
                  </div>
                  <p className='text-xs text-muted-foreground'>
                    The actual price customers will pay when on sale. This is NOT a deduction — enter the final sale price directly (e.g., if regular price is $100 and you want to sell at $80, enter $80 here). A "SALE" badge will appear on the product.
                  </p>
                </div>
              </div>
            </div>

            {/* Product Options Section */}
            <div className='space-y-4 pt-4 border-t'>
              <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
                <Store className='size-4' />
                Product Options
              </div>

              <div className='grid sm:grid-cols-2 gap-4'>
                {/* In Stock Toggle */}
                <div className='flex items-center justify-between rounded-lg border p-4'>
                  <div className='flex items-center gap-3'>
                    <div className='flex size-9 items-center justify-center rounded-md bg-emerald-500/10'>
                      <PackageCheck className='size-4 text-emerald-600' />
                    </div>
                    <div>
                      <Label className='text-sm font-medium'>In Stock</Label>
                      <p className='text-xs text-muted-foreground'>
                        Product is available
                      </p>
                    </div>
                  </div>
                  <Switch
                    checked={formData.inStock}
                    onCheckedChange={(checked) =>
                      handleInputChange('inStock', checked)
                    }
                  />
                </div>

                {/* Weightable Toggle */}
                <div className='flex items-center justify-between rounded-lg border p-4'>
                  <div className='flex items-center gap-3'>
                    <div className='flex size-9 items-center justify-center rounded-md bg-blue-500/10'>
                      <Scale className='size-4 text-blue-600' />
                    </div>
                    <div>
                      <Label className='text-sm font-medium'>Weightable</Label>
                      <p className='text-xs text-muted-foreground'>
                        Sold by weight
                      </p>
                    </div>
                  </div>
                  <Switch
                    checked={formData.isWeightable}
                    onCheckedChange={(checked) =>
                      handleInputChange('isWeightable', checked)
                    }
                  />
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Footer */}
      <div className='sticky bottom-0 z-10 -mx-6 px-6 py-3 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 border-t'>
        <div className='flex items-center justify-end gap-2'>
          <Button variant='outline' size='sm' onClick={handleBack}>
            Cancel
          </Button>
          <Button
            size='sm'
            onClick={handleCreateProduct}
            disabled={loading}
          >
            <Save className='size-4 mr-1.5' />
            {loading ? 'Creating...' : 'Create Product'}
          </Button>
        </div>
      </div>
    </div>
  );
}

