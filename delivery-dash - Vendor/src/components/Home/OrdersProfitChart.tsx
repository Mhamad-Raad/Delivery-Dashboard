"use client"

import { Area, AreaChart, CartesianGrid, XAxis, YAxis } from "recharts"
import { TrendingUp, TrendingDown, ShoppingCart, DollarSign, Receipt, Wallet } from "lucide-react"
import { useTranslation } from "react-i18next"

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/components/ui/chart"
import { formatPrice } from "@/lib/formatNumbers"

interface OrderData {
  day: string;
  orders: number;
  profit: number;
}

interface OrdersProfitChartProps {
  data: OrderData[];
}

const chartConfig = {
  profit: {
    label: "Profit",
    color: "var(--chart-2)",
  },
} satisfies ChartConfig

function OrdersProfitChart({ data }: OrdersProfitChartProps) {
  const { t } = useTranslation()
  const totalOrders = data.reduce((sum, item) => sum + item.orders, 0);
  const totalProfit = data.reduce((sum, item) => sum + item.profit, 0);
  const serviceFee = totalProfit * 0.03;
  const netProfit = totalProfit - serviceFee;

  // Calculate trend - compare first half to second half of available data
  const midPoint = Math.ceil(data.length / 2);
  const firstHalf = data.slice(0, midPoint).reduce((sum, item) => sum + item.profit, 0);
  const secondHalf = data.slice(midPoint).reduce((sum, item) => sum + item.profit, 0);
  const trendPercentage = firstHalf > 0 ? ((secondHalf - firstHalf) / firstHalf * 100).toFixed(1) : secondHalf > 0 ? '100.0' : '0.0';
  const isTrendingUp = secondHalf >= firstHalf;

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <div>
          <CardTitle className="text-lg">{t('home.profitOverviewTitle')}</CardTitle>
          <p className="text-sm text-muted-foreground">{t('home.profitOverviewSubtitle')}</p>
        </div>
        <div className={`flex items-center gap-1 text-sm font-medium ${isTrendingUp ? 'text-primary' : 'text-destructive'}`}>
          {isTrendingUp ? <TrendingUp className="h-4 w-4" /> : <TrendingDown className="h-4 w-4" />}
          {trendPercentage}%
        </div>
      </CardHeader>
      <CardContent className="pt-4">
        <div className="grid grid-cols-1 xl:grid-cols-[1fr_280px] gap-6">
          {/* Chart Section */}
          <ChartContainer config={chartConfig} className="h-[250px] w-full">
            <AreaChart
              accessibilityLayer
              data={data}
              margin={{
                left: 0,
                right: 8,
                top: 8,
                bottom: 0,
              }}
            >
              <defs>
                <linearGradient id="profitGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="var(--color-profit)" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="var(--color-profit)" stopOpacity={0.05} />
                </linearGradient>
              </defs>
              <CartesianGrid vertical={false} strokeDasharray="3 3" className="stroke-muted/50" />
              <XAxis
                dataKey="day"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                tickFormatter={(value) => value.replace('Dec ', '')}
                className="text-xs"
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                tickFormatter={(value) => `$${value}`}
                className="text-xs"
                width={60}
              />
              <ChartTooltip
                cursor={{ stroke: 'var(--color-profit)', strokeDasharray: '3 3' }}
                content={<ChartTooltipContent indicator="line" />}
              />
              <Area
                dataKey="profit"
                type="monotone"
                stroke="var(--color-profit)"
                strokeWidth={2}
                fill="url(#profitGradient)"
                dot={{ fill: "var(--color-profit)", strokeWidth: 0, r: 3 }}
                activeDot={{ r: 5, strokeWidth: 2, stroke: "var(--background)" }}
              />
            </AreaChart>
          </ChartContainer>

          {/* Stats Section */}
          <div className="flex flex-col justify-center gap-3 xl:border-l xl:pl-6">
            <div className="grid grid-cols-2 xl:grid-cols-1 gap-3">
              <div className="flex items-center gap-3 p-3 rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors">
                <div className="p-2.5 rounded-lg bg-chart-1/15">
                  <ShoppingCart className="h-4 w-4 text-chart-1" />
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">{t('home.totalOrders')}</p>
                  <p className="text-xl font-bold tabular-nums">{totalOrders}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 p-3 rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors">
                <div className="p-2.5 rounded-lg bg-chart-2/15">
                  <DollarSign className="h-4 w-4 text-chart-2" />
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">{t('home.grossProfit')}</p>
                  <p className="text-xl font-bold tabular-nums">{formatPrice(totalProfit)}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 p-3 rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors">
                <div className="p-2.5 rounded-lg bg-destructive/15">
                  <Receipt className="h-4 w-4 text-destructive" />
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">{t('home.serviceFee')}</p>
                  <p className="text-xl font-bold tabular-nums text-destructive">-{formatPrice(serviceFee)}</p>
                </div>
              </div>
              <div className="flex items-center gap-3 p-3 rounded-lg bg-primary/5 border border-primary/20 hover:bg-primary/10 transition-colors">
                <div className="p-2.5 rounded-lg bg-primary/15">
                  <Wallet className="h-4 w-4 text-primary" />
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">{t('home.netProfit')}</p>
                  <p className="text-xl font-bold tabular-nums text-primary">{formatPrice(netProfit)}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}

export default OrdersProfitChart;
